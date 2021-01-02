using System.Collections.Generic;
using SeeSharp.Core;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Core.Image;
using MisForCorrelatedBidir.Common;
using System;

namespace MisForCorrelatedBidir.VcmExperiment {
    class RadiusExperiment : SeeSharp.Experiments.ExperimentFactory {
        public override FrameBuffer.Flags FrameBufferFlags => FrameBuffer.Flags.SendToTev;
        public SceneConfig Scene { get; init; }

        protected virtual int Samples => 4;
        protected virtual int PrepassSamples => 16;

        public RadiusExperiment(SceneConfig scene, bool runQuickTest=false) {
            Scene = scene;
            this.runQuickTest = runQuickTest;
        }
        bool runQuickTest;

        public override List<Method> MakeMethods() => new() {
            new Method("Radius-x2", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 2
                }
            }),
            new Method("Radius-x10", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 10
                }
            }),
            new Method("Radius-x100", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 100
                }
            }),
            new Method("Radius-x05", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 2
                }
            }),
            new Method("Radius-x01", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 10
                }
            }),
            new Method("Radius-x001", new PdfRatioVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 100
                }
            }),
        };

        public override Scene MakeScene() => Scene.MakeScene();
        public override Integrator MakeReferenceIntegrator() => Scene.MakeReferenceIntegrator();
    }
}