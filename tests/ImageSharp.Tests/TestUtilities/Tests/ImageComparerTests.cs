// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

    using Moq;

    using SixLabors.Primitives;

    using Xunit;
    using Xunit.Abstractions;

    public class ImageComparerTests
    {
        public ImageComparerTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ITestOutputHelper Output { get; }

        [Theory]
        [WithTestPatternImages(100,100,PixelTypes.Rgba32, 0.0001f, 1)]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32, 0, 0)]
        public void TolerantImageComparer_ApprovesPerfectSimilarity<TPixel>(
            TestImageProvider<TPixel> provider,
            float imageTheshold,
            int pixelThreshold)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    var comparer = ImageComparer.Tolerant(imageTheshold, pixelThreshold);
                    comparer.VerifySimilarity(image, clone);
                }
            }
        }

        [Theory]
        [WithTestPatternImages(110, 110, PixelTypes.Rgba32)]
        public void TolerantImageComparer_ApprovesSimilarityBelowTolerance<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ImagingTestCaseUtility.ModifyPixel(clone, 0, 0, 1);

                    var comparer = ImageComparer.Tolerant();
                    comparer.VerifySimilarity(image, clone);
                }
            }
        }

        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32)]
        public void TolerantImageComparer_DoesNotApproveSimilarityAboveTolerance<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ImagingTestCaseUtility.ModifyPixel(clone, 3, 1, 2);

                    var comparer = ImageComparer.Tolerant();

                    ImagePixelsAreDifferentException ex = Assert.ThrowsAny<ImagePixelsAreDifferentException>(
                        () =>
                            {
                                comparer.VerifySimilarity(image, clone);
                            });
                    PixelDifference diff = ex.Reports.Single().Differences.Single();
                    Assert.Equal(new Point(3, 1), diff.Position);
                }
            }
        }
        
        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32)]
        public void TolerantImageComparer_TestPerPixelThreshold<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ImagingTestCaseUtility.ModifyPixel(clone, 0, 0, 10);
                    ImagingTestCaseUtility.ModifyPixel(clone, 1, 0, 10);
                    ImagingTestCaseUtility.ModifyPixel(clone, 2, 0, 10);

                    var comparer = ImageComparer.Tolerant(pixelThresholdHammingDistance: 42);
                    comparer.VerifySimilarity(image, clone);
                }
            }
        }

        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32, 99, 100)]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32, 100, 99)]
        public void VerifySimilarity_ThrowsOnSizeMismatch<TPixel>(TestImageProvider<TPixel> provider, int w, int h)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone(ctx => ctx.Resize(w, h)))
                {
                    ImageDimensionsMismatchException ex = Assert.ThrowsAny<ImageDimensionsMismatchException>(
                        () =>
                            {
                                ImageComparer comparer = Mock.Of<ImageComparer>();
                                comparer.VerifySimilarity(image, clone);
                            });
                    this.Output.WriteLine(ex.Message);
                }
            }
        }


        [Theory]
        [WithFile(TestImages.Gif.Giphy, PixelTypes.Rgba32)]
        public void VerifySimilarity_WhenAnImageFrameIsDifferent_Reports<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ImagingTestCaseUtility.ModifyPixel(clone.Frames[0], 42, 43, 1);

                    IEnumerable<ImageSimilarityReport> reports = ImageComparer.Exact.CompareImages(image, clone);

                    PixelDifference difference = reports.Single().Differences.Single();
                    Assert.Equal(new Point(42, 43), difference.Position);
                }
            }
        }


        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32)]
        public void ExactComparer_ApprovesExactEquality<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ExactImageComparer.Instance.CompareImages(image, clone);
                }
            }
        }
        
        [Theory]
        [WithTestPatternImages(100, 100, PixelTypes.Rgba32)]
        public void ExactComparer_DoesNotTolerateAnyPixelDifference<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                using (Image<TPixel> clone = image.Clone())
                {
                    ImagingTestCaseUtility.ModifyPixel(clone, 42, 24, 1);
                    ImagingTestCaseUtility.ModifyPixel(clone, 7, 93, 1);

                    IEnumerable<ImageSimilarityReport> reports = ExactImageComparer.Instance.CompareImages(image, clone);
                    
                    this.Output.WriteLine(reports.Single().ToString());
                    PixelDifference[] differences = reports.Single().Differences;
                    Assert.Equal(2, differences.Length);
                    Assert.Contains(differences, d => d.Position == new Point(42, 24));
                    Assert.Contains(differences, d => d.Position == new Point(7, 93));
                }
            }
        }
    }
}