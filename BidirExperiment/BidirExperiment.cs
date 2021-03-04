using System;
using System.Collections.Generic;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.BidirExperiment {
    class BidirExperiment : SeeSharp.Experiments.Experiment {
        public int Samples = 4;
        public int SplitFactor = 16;

        public override List<Method> MakeMethods() {
            return new List<Method>() {
                new Method("PathTracer", new PathTracer() {
                    TotalSpp = Samples * 2,
                }),
                new Method("Bidir", new ClassicBidir() {
                    NumIterations = Samples,
                    NumShadowRays = 1, RenderTechniquePyramid = false,
                }),
                new Method("BidirSplit", new ClassicBidir() {
                    NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),

                new Method("UpperBound", new UpperBoundBidir() {
                    NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),
                new Method("VarAware", new VarAwareMisBidir() {
                    NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),
                new Method("PdfRatio", new PdfRatioBidir() {
                    NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(1 * MathF.PI / 180) }
                })
            };
        }
    }
}