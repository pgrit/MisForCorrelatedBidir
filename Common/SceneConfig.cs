using SeeSharp.Core;
using SeeSharp.Integrators;
using System;

namespace MisForCorrelatedBidir.Common {
    public abstract class SceneConfig {
        public abstract Scene MakeScene();
        public abstract Integrator MakeReferenceIntegrator();
        public virtual int MaxDepth => 5;
    }

    public class LampCaustic : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 10000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };
        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/LampCaustic/LampCaustic.json");
    }

    public class LampCausticNoShade : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 10000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/LampCaustic/LampCausticNoShade.json");
    }

    public class ModernHall : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 5000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/ModernHall/ModernHall.json");
    }

    public class TargetPractice : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 512, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/TargetPractice/target_practice.json");
    }

    public class ContemporaryBathroom : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 20000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/ContemporaryBathroom/contemporary_bathroom.json");
    }

    public class HomeOffice : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 20000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/HomeOffice/office.json");
    }

    public class RoughGlasses : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 20000,
            BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses.json");
    }

    public class RoughGlassesLens : RoughGlasses {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-Lens.json");
    }

    public class RoughGlassesNarrow : RoughGlasses {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-NarrowFov.json");
    }

    public class RoughGlassesWide : RoughGlasses {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-WideFov.json");
    }

    public class RoughGlassesIndirect : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new Common.PdfRatioVcm() {
            MaxDepth = MaxDepth, NumIterations = 20000,
            BaseSeedCamera = 971612, BaseSeedLight = 175037,
            RadiusInitializer = new RadiusInitCombined {
                Candidates = new() {
                    new RadiusInitFov { ScalingFactor = MathF.Pow(5 * MathF.PI / 180, 2) },
                    new RadiusInitScene { ScalingFactor = 0.01f }
                }
            }
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-Indirect.json");
    }

    public class RoughGlassesIndirectLens : RoughGlassesIndirect {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-Indirect-Lens.json");
    }

    public class RoughGlassesIndirectNarrow : RoughGlassesIndirect {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-Indirect-NarrowFov.json");
    }

    public class RoughGlassesIndirectWide : RoughGlassesIndirect {
        public override Scene MakeScene()
        => Scene.LoadFromFile("../Scenes/RoughGlasses/RoughGlasses-Indirect-WideFov.json");
    }

    public class IndirectRoom : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 10000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/IndirectRoom/IndirectRoom.json");
    }

    public class LivingRoom : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 10000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/LivingRoom/LivingRoomVCM.json");
    }

    public class MinimalistWhiteRoom : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 10000, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/MinimalistWhiteRoom/MinWhite.json");
    }
}