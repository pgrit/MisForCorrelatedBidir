This is the supplemental of the paper "Correlated Multiple Importance Sampling for Bidirectional Rendering Algorithms". It contains the following data:

1. "SeeSharp", the renderer used for our experiments (just the basic framework, nothing relevant to the paper going on in there)
2. "Experiments", the actual implementation of our two use-cases
3. "Scenes", a few select test scenes from the paper, to allow reproduction of (some of) the results
4. "ComparisonFull.mp4", a video version of Figure 3 in the paper
5. "Results", full resolution .png images of all methods, along with overview .pdf files and static .html webpages to browse, view, and compare more easily

Running the experiments yourself requires the following:

- A C++11 compiler
- Embree3 (and TBB)
- CMake
- .NET Core >= 3.0

The nasty bit is getting SeeSharp/SeeCore, the wrapper around the C++ dependencies, to compile using CMake. Then, you also need to add SeeSharp/dist to the linker search path. For Linux users, there is a handy script in `SeeSharp/setup_env.sh`. Windows users can simply add `SeeSharp/dist` to the PATH.

From there on out, it should be smooth sailing. Simply run: `cd Experiments/Vcm && dotnet run -c Release`.
