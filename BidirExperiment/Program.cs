using System.Collections.Generic;
using SeeSharp.Experiments;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.BidirExperiment {
    class Program {
        static void Main(string[] args) {
            int resolutionScale = 2;
            int splitfactor = 64;
            var bench = new Benchmark(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                {"ModernHall", new BidirExperiment(new ModernHall()){
                    SplitFactor = splitfactor
                }},
                {"LivingRoom", new BidirExperiment(new LivingRoom()){
                    SplitFactor = splitfactor
                }},
                {"BananaRange", new BidirExperiment(new BananaRange()){
                    SplitFactor = splitfactor
                }},
                {"ContemporaryBathroom", new BidirExperiment(new ContemporaryBathroom()){
                    SplitFactor = splitfactor
                }},
                {"HomeOffice", new BidirExperiment(new HomeOffice()){
                    SplitFactor = splitfactor
                }},
                {"RoughGlasses", new BidirExperiment(new RoughGlasses()){
                    SplitFactor = splitfactor
                }},
                {"RoughGlassesIndirect", new BidirExperiment(new RoughGlassesIndirect()){
                    SplitFactor = splitfactor
                }},
                {"IndirectRoom", new BidirExperiment(new IndirectRoom()){
                    SplitFactor = splitfactor
                }},
                {"MinimalistWhiteRoom", new BidirExperiment(new MinimalistWhiteRoom()){
                    SplitFactor = splitfactor
                }},
            }, 640 * resolutionScale, 480 * resolutionScale);
            bench.Run(forceReference: false);
        }
    }
}
