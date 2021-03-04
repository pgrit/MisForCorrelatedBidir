using System.Collections.Generic;
using MisForCorrelatedBidir.Common;
using System;

namespace MisForCorrelatedBidir.VcmExperiment {
    class RadiusExperiment : SeeSharp.Experiments.Experiment {
        bool runQuickTest;
        int samples;

        public RadiusExperiment(bool runQuickTest = false, int samples = 4) {
            this.runQuickTest = runQuickTest;
            this.samples = samples;
        }

        public override List<Method> MakeMethods() => new() {
            new Method("Radius-x2", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 2
                }
            }),
            new Method("Radius-x10", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 10
                }
            }),
            new Method("Radius-x100", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) * 100
                }
            }),
            new Method("Radius-x05", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 2
                }
            }),
            new Method("Radius-x01", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 10
                }
            }),
            new Method("Radius-x001", new PdfRatioVcm() {
                NumIterations = samples, MergePrimary = false,
                RenderTechniquePyramid = false,
                RadiusInitializer = new RadiusInitFov {
                    ScalingFactor = MathF.Tan(MathF.PI / 180) / 100
                }
            }),
        };
    }
}