using System.Collections.Generic;
using SeeSharp.Experiments;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.BidirExperiment {
    class Program {

        static void RunBench(int splitfactor) {
            int resolutionScale = 2;
            var bench = new Benchmark(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                {"ModernHall", new BidirExperiment(new ModernHall()){
                    SplitFactor = splitfactor
                }},
                {"LivingRoom", new BidirExperiment(new LivingRoom()){
                    SplitFactor = splitfactor
                }},
                {"TargetPractice", new BidirExperiment(new TargetPractice()){
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
                {"LampCaustic", new BidirExperiment(new LampCaustic()){
                    SplitFactor = splitfactor
                }},
            }, 640 * resolutionScale, 480 * resolutionScale) { DirectoryName = $"Results-x{splitfactor}" };
            bench.Run(forceReference: false);
        }

        static void Main(string[] args) {
            RunBench(10);
            RunBench(50);
            RunBench(100);
        }
    }
}
