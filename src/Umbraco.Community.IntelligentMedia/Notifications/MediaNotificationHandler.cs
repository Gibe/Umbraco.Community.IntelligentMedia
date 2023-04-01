using Azure.AI.Vision.Core.Input;
using Azure.AI.Vision.Core.Options;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Community.IntelligentMedia.Notifications
{
	public class MediaNotificationHandler : INotificationHandler<MediaSavingNotification>
	{
		private readonly ILogger<MediaNotificationHandler> _logger;
		private readonly IDataTypeService _dataTypeService;
		private readonly MediaFileManager _mediaFileManager;
		private readonly IMediaService _mediaService;

		public MediaNotificationHandler(ILogger<MediaNotificationHandler> logger, IDataTypeService dataTypeService, MediaFileManager mediaFileManager, IMediaService mediaService)
		{
			_logger = logger;
			_dataTypeService = dataTypeService;
			_mediaFileManager = mediaFileManager;
			_mediaService = mediaService;
		}

		public void Handle(MediaSavingNotification notification)
		{
			foreach (var mediaItem in notification.SavedEntities)
			{
				if (mediaItem.ContentType.Alias.Equals("Image"))
				{
					if (mediaItem.Properties["umbracoFile"].PropertyType.PropertyEditorAlias.Equals("Umbraco.ImageCropper"))
					{
						var dataType = _dataTypeService.GetByEditorAlias("Umbraco.ImageCropper");
						var config = dataType.First().Configuration as ImageCropperConfiguration;
						var ratios = config.Crops.Select(c => new CropRatio(c));

						var value = JsonSerializer.Deserialize<ImageValue>(mediaItem.Properties["umbracoFile"].Values.First()
							.EditedValue.ToString());



						var serviceOptions = new VisionServiceOptions(
							"https://westeurope.api.cognitive.microsoft.com/", "");

						//get the image file from local disk as a stream using Umbraco MediaFileManager
						var imageStream = _mediaFileManager.GetFile(mediaItem, out string mediaPath);
						
						using (imageStream)
						{
							using (var memStream = new MemoryStream())
							{
								imageStream.CopyTo(memStream);

								var imageSource = VisionSource.FromPayload(memStream.ToArray(), "image/jpeg");

								var analysisOptions = new ImageAnalysisOptions
								{
									Features = ImageAnalysisFeature.CropSuggestions,
									//CroppingAspectRatios = ratios.Select(r => r.Ratio).ToArray()
								};

								using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

								var result = analyzer.Analyze();

								if (result.Reason == ImageAnalysisResultReason.Analyzed)
								{
									if (result.CropSuggestions != null)
									{
										var focalPointBox = result.CropSuggestions.First();

									}
								}
							}
						}
					}
					// Get the crops
					
					// Convert the crops to ratio numbers

					// Get the image

					// Check size of image

					// Send the image

					// Modify the image crops
				}
			}
		}

		
	}

	public class ImageValue
	{
		public ImageValue()
		{
			Crops = new List<Crop>();
		}

		public string Src { get; set; }
		public ImageCropperValue.ImageCropperFocalPoint FocalPoint { get; set; }
		public List<Crop> Crops { get; set; }
	}

	public class Crop
	{
		public string Alias { get; set; }
		public ImageCropperValue.ImageCropperCropCoordinates Coordinates { get; set; }
	}

	public class CropRatio
	{
		public CropRatio(ImageCropperConfiguration.Crop crop)
		{
			Crop = crop;
			Ratio = 
				Math.Min(
					Math.Max(
							(double)crop.Width / (double)crop.Height
						, 0.75)
					, 1.5);
		}

		public ImageCropperConfiguration.Crop Crop { get; }
		public double Ratio { get; }
	}
}
