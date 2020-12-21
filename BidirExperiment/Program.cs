using System.Collections.Generic;
using SeeSharp.Experiments;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.BidirExperiment {
    class Program {
        static void Main(string[] args) {
            int resolutionScale = 2;
            var bench = new Benchmark(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                { "ModernHall", new BidirExperiment(new ModernHall()) },
                { "LivingRoom", new BidirExperiment(new LivingRoom()) },
                { "BananaRange", new BidirExperiment(new BananaRange()) },
                { "ContemporaryBathroom", new BidirExperiment(new ContemporaryBathroom()) },
                { "HomeOffice", new BidirExperiment(new HomeOffice()) },
                { "RoughGlasses", new BidirExperiment(new RoughGlasses()) },
                { "RoughGlassesIndirect", new BidirExperiment(new RoughGlassesIndirect()) },
                { "IndirectRoom", new BidirExperiment(new IndirectRoom()) },
                { "MinimalistWhiteRoom", new BidirExperiment(new MinimalistWhiteRoom()) },
            }, 640 * resolutionScale, 480 * resolutionScale);
            bench.Run(forceReference: false);
        }
    }
}
