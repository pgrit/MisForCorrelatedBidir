using System;
using System.Collections.Generic;
using SeeSharp.Core;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Core.Image;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.BidirExperiment {
    class BidirExperiment : SeeSharp.Experiments.ExperimentFactory {
        public override FrameBuffer.Flags FrameBufferFlags => FrameBuffer.Flags.SendToTev;

        protected virtual int MaxDepth => 5;
        protected virtual int Samples => 4;
        protected virtual int SplitFactor => 16;

        public SceneConfig Scene { get; init; }

        public BidirExperiment(SceneConfig scene) {
            Scene = scene;
        }

        public override List<Method> MakeMethods() {
            return new List<Method>() {
                new Method("PathTracer", new PathTracer() {
                    MaxDepth = (uint)MaxDepth, TotalSpp = Samples * 2,
                }),
                new Method("Bidir", new ClassicBidir() {
                    MaxDepth = MaxDepth, NumIterations = Samples,
                    NumShadowRays = 1, RenderTechniquePyramid = false,
                }),
                new Method("BidirSplit", new ClassicBidir() {
                    MaxDepth = MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),

                new Method("UpperBound", new UpperBoundBidir() {
                    MaxDepth = MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),
                new Method("VarAware", new VarAwareMisBidir() {
                    MaxDepth = MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                }),

                new Method("PdfRatio", new PdfRatioBidir() {
                    MaxDepth = MaxDepth, NumIterations = Samples,
                    NumShadowRays = SplitFactor, RenderTechniquePyramid = false,
                    Mode = PdfRatioBidir.FootprintMode.CameraAngle,
                    ScalingFactor = MathF.Pow(10 * MathF.PI / 180, 2)
                })
            };
        }

        public override Scene MakeScene() => Scene.MakeScene();
        public override Integrator MakeReferenceIntegrator() => Scene.MakeReferenceIntegrator();
    }
}