using SeeSharp.Experiments;
using System.Collections.Generic;

namespace MisForCorrelatedBidir.VideoGen {
    class Program {
        static void Main(string[] args) {
            int resolutionScale = 1;
            var bench = new Benchmark(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
            }, 640 * resolutionScale, 480 * resolutionScale);

            for (int frame = 1; frame <= 50; ++frame) {
                var name = $"Frame{frame:0000}";
                bench.Experiments.Add(name, new Frame(frame));
            }

            bench.Run(forceReference: false);
        }
    }
}
