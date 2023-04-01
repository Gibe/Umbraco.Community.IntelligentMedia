using Azure.AI.Vision.Core.Input;
using Azure.AI.Vision.Core.Options;
using Azure.AI.Vision.ImageAnalysis;

public class Program
{
	static void AnalyzeImage()
	{
		var serviceOptions = new VisionServiceOptions(
				"https://westeurope.api.cognitive.microsoft.com/", "");

		var imageSource = VisionSource.FromUrl(
				new Uri("https://func-wolves-playmaker-imageresizer-prod-uksouth.azurewebsites.net/api/media/yzihyync/collins-france.jpg"));

		var analysisOptions = new ImageAnalysisOptions
		{
			Features = ImageAnalysisFeature.Caption | ImageAnalysisFeature.Text | ImageAnalysisFeature.CropSuggestions,

			Language = "en",

			GenderNeutralCaption = true,

			CroppingAspectRatios = new[] {0.75d, 1.5d}
		};

		using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

		var result = analyzer.Analyze();

		if (result.Reason == ImageAnalysisResultReason.Analyzed)
		{
			if (result.Caption != null)
			{
				Console.WriteLine(" Caption:");
				Console.WriteLine($"   \"{result.Caption.Content}\", Confidence {result.Caption.Confidence:0.0000}");
			}

			if (result.Text != null)
			{
				Console.WriteLine($" Text:");
				foreach (var line in result.Text.Lines)
				{
					string pointsToString = "{" + string.Join(',', line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
					Console.WriteLine($"   Line: '{line.Content}', Bounding polygon {pointsToString}");

					foreach (var word in line.Words)
					{
						pointsToString = "{" + string.Join(',', word.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
						Console.WriteLine($"     Word: '{word.Content}', Bounding polygon {pointsToString}, Confidence {word.Confidence:0.0000}");
					}
				}
			}

			if (result.CropSuggestions != null)
			{
				Console.WriteLine(" Crops:");
				foreach (var sug in result.CropSuggestions)
				{
					Console.WriteLine("    Crop:" + sug.AspectRatio + " " + sug.BoundingBox);
				}
			}
		}
		else if (result.Reason == ImageAnalysisResultReason.Error)
		{
			var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
			Console.WriteLine(" Analysis failed.");
			Console.WriteLine($"   Error reason : {errorDetails.Reason}");
			Console.WriteLine($"   Error code : {errorDetails.ErrorCode}");
			Console.WriteLine($"   Error message: {errorDetails.Message}");
		}
	}

	static void Main()
	{
		try
		{
			AnalyzeImage();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
}