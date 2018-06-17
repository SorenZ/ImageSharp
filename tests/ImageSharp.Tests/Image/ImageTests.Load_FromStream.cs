﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

using Xunit;
// ReSharper disable InconsistentNaming

namespace SixLabors.ImageSharp.Tests
{
    public partial class ImageTests
    {
        /// <summary>
        /// Tests the <see cref="Image"/> class.
        /// </summary>
        public class Load_FromStream : ImageLoadTestBase
        {
            [Fact]
            public void BasicCase()
            {
                var img = Image.Load(this.TopLevelConfiguration, this.DataStream);

                Assert.NotNull(img);
                Assert.Equal(this.TestFormat.Sample<Rgba32>(), img);

                this.TestFormat.VerifyDecodeCall(this.Marker, this.TopLevelConfiguration);
            }

            [Fact]
            public void NonDefaultPixelTypeImage()
            {
                var img = Image.Load<Rgb24>(this.TopLevelConfiguration, this.DataStream);

                Assert.NotNull(img);
                Assert.Equal(this.TestFormat.Sample<Rgb24>(), img);

                this.TestFormat.VerifyDecodeCall(this.Marker, this.TopLevelConfiguration);
            }

            [Fact]
            public void NonSeekableStream()
            {
                var stream = new NoneSeekableStream(this.DataStream);
                var img = Image.Load<Rgba32>(this.TopLevelConfiguration, stream);

                Assert.NotNull(img);

                this.TestFormat.VerifyDecodeCall(this.Marker, this.TopLevelConfiguration);
            }

            [Fact]
            public void UseLocalConfiguration()
            {
                Stream stream = new MemoryStream();
                var img = Image.Load<Rgba32>(this.LocalConfiguration, stream);

                Assert.NotNull(img);

                this.localDecoder.Verify(x => x.Decode<Rgba32>(this.LocalConfiguration, stream));
            }
            
            [Fact]
            public void UseCustomDecoder()
            {
                Stream stream = new MemoryStream();
                var img = Image.Load<Rgba32>(this.TopLevelConfiguration, stream, this.localDecoder.Object);

                Assert.NotNull(img);
                this.localDecoder.Verify(x => x.Decode<Rgba32>(this.TopLevelConfiguration, stream));
            }

            [Fact]
            public void LoadFromPixelData_Pixels()
            {
                var img = Image.LoadPixelData<Rgba32>(new Rgba32[] {
                                                                           Rgba32.Black, Rgba32.White,
                                                                           Rgba32.White, Rgba32.Black,
                                                                       }, 2, 2);

                Assert.NotNull(img);
                Assert.Equal(Rgba32.Black, img[0, 0]);
                Assert.Equal(Rgba32.White, img[0, 1]);

                Assert.Equal(Rgba32.White, img[1, 0]);
                Assert.Equal(Rgba32.Black, img[1, 1]);
            }

            // TODO: This should be a png decoder test!
            [Fact]
            public void LoadsImageWithoutThrowingCrcException()
            {
                var image1Provider = TestImageProvider<Rgba32>.File(TestImages.Png.VersioningImage1);

                using (Image<Rgba32> img = image1Provider.GetImage())
                {
                    Assert.Equal(166036, img.Frames.RootFrame.GetPixelSpan().Length);
                }
            }
        }
    }
}