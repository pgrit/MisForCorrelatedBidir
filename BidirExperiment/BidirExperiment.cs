using System;
using System.Collections.Generic;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using MisForCorrelatedBidir.Common;
using SeeSharp.Image;
using SeeSharp;

namespace MisForCorrelatedBidir.BidirExperiment {
    class BidirExperiment : SeeSharp.Experiments.ExperimentFactory {
        public override FrameBuffer.Flags FrameBufferFlags => FrameBuffer.Flags.SendToTev;

        public int Samples = 4;
        public int SplitFactor = 16;

        public SceneConfig Scene { get; init; }

        public BidirExperiment(SceneConfig scene) {
            Scene = scene;
        }

        public override List<Method> MakeMethods() {
            return new List<Method>() {
                new Method("PathTracer", new PathTracer() {
                    MaxDepth = (uint)Scene.MaxDepth, TotalSpp = Samples * 2,
                }),
                new Method("Bidir", new ClassicBidir() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                    NumShadowRays = 1, RenderTechniquePyramid = false,
                }),
                new Method("BidirSplit", new ClassicBidir() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),

                new Method("UpperBound", new UpperBoundBidir() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),
                new Method("VarAware", new VarAwareMisBidir() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),
                new Method("PdfRatio", new PdfRatioBidir() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(1 * MathF.PI / 180) }
                })
            };
        }

        public override Scene MakeScene() => Scene.MakeScene();
        public override Integrator MakeReferenceIntegrator() => Scene.MakeReferenceIntegrator();
    }
}