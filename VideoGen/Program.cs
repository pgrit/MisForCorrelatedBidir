using SeeSharp.Experiments;
using System.Collections.Generic;
using System.Diagnostics;

namespace MisForCorrelatedBidir.VideoGen {
    class Program {
        static void Main(string[] args) {
            int resolutionScale = 1;

            List<SceneConfig> frames = new();
            for (int frame = 1; frame <= 50; ++frame) {
                frames.Add(new Frame(frame));
            }

            Benchmark benchmark = new(new ExperimentSetup(), frames, "Results",
                640 * resolutionScale, 480 * resolutionScale);

            benchmark.Run();

            var p1 = Process.Start("python", "./makevideo.py");
            var p2 = Process.Start("python", "./makefigure.py");
            p1.WaitForExit();
            p2.WaitForExit();
        }
    }
}
