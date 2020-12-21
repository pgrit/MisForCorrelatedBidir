using SeeSharp.Core;
using SeeSharp.Integrators;

namespace MisForCorrelatedBidir.Common {
    public abstract class SceneConfig {
        public abstract Scene MakeScene();
        public abstract Integrator MakeReferenceIntegrator();
        public virtual int MaxDepth => 5;
    }

    public class ModernHall : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 512, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/ModernHall/ModernHall.json");
    }

    public class BananaRange : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 128, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/BananaRange/banana_range.json");
    }

    public class ContemporaryBathroom : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 2048, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/ContemporaryBathroom/contemporary_bathroom.json");
    }

    public class HomeOffice : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 2048, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/HomeOffice/office.json");
    }

    public class RoughGlasses : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 500,//5000,
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
            MaxDepth = MaxDepth, NumIterations = 1000,//10000,
            BaseSeedCamera = 971612, BaseSeedLight = 175037
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
            MaxDepth = MaxDepth, NumIterations = 1024, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/IndirectRoom/IndirectRoom.json");
    }

    public class LivingRoom : SceneConfig {
        public override int MaxDepth => 10;
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.VertexConnectionAndMerging() {
            MaxDepth = MaxDepth, NumIterations = 2048, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/LivingRoom/LivingRoomVCM.json");
    }

    public class MinimalistWhiteRoom : SceneConfig {
        public override Integrator MakeReferenceIntegrator()
        => new SeeSharp.Integrators.Bidir.ClassicBidir() {
            MaxDepth = MaxDepth, NumIterations = 1024, BaseSeedCamera = 971612, BaseSeedLight = 175037
        };

        public override Scene MakeScene() => Scene.LoadFromFile("../Scenes/MinimalistWhiteRoom/MinWhite.json");
    }
}