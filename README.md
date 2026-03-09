Cloning the project from Github should have most config for ML2 already setup in the Unity scene. I recommend watching this video to understand what is going on:
https://youtu.be/KqH0zv3e2AY?si=qMkECZtkaS4p3niX

It will also walk through dowloading Magic Leap Hub and the Unity package which you will need to do. The app simulator at the end of the video is now deprecated,
as far as I know the only way to test an application is build it to the ML2.

This video is the most important for understanding a lot of the code, specifically any of the scripts about anchors:
https://youtu.be/fOLay379LcE?si=X0jnVr8DdRX11IkT

The scripts PlaneClassifier.cs and PlaneConfigurationManager.cs are from my attempts to use plane detection. This could end up being a simpler and more reliable 
route away from meshing, if having every surface be a horizontal or vertical plane is sufficient complexity for the simulation. It was giving me a lot of trouble to setup so
that is why I moved to trying meshing.

This is the documentation for ML2 and Unity that was used for meshing (TriangleMeshing.cs) and any errors that come up:
https://developer-docs.magicleap.cloud/docs/guides/unity-openxr/meshing/unity-openxr-meshing/
^Just be sure to only reference the OpenXR section, as the other is deprecated.

