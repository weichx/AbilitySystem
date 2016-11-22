-----====== WHAT'S NEW ? ======-----

	ALLSKY V2

	- New Skies -
	23 extra skies added for a new total of 140!  Fantasy skies at different times of day with very dramatic clouds and colours. Cartoon skies in distinctive painterly, watercolour, airbrushed or pixelated styles.  Space skyboxes with epic, colourful nebulae.  More overcast skies for moody scenes.

	- New Format -
	Skies re-exported in a new format. From equi-rectangular to 6 sided. This results in a small quality increase, and should make
	life easier for developers on certain platforms.

		! Be sure to backup your old AllSky package if you want to retain the equi-rectangular versions !

	- Lighting Examples -
	All 140 skies now have a low poly demo environment with example lighting and fog pass. Great if you want a reference for colour and luminance values.
	Please set your project to the deferred rendering path and linear lighting color space to view these as intended.


-----====== ALLSKY ======-----

	A palette of 140 skies for Unity!
	 
	For lighting artists, environment artists and indie developers looking for a wide suite of skies to light their environments.

	Many weather and cloud types: Clear, overcast, summery, stormy, autumnal, hazy, epic, foggy, cumulus.  

	Lighting from day to night: Twilight, sunset, multiple times of day, multiple times of night, skyglow.

	Various styles: Cartoon, Fantasy & Space Nebulae.
  

-----====== TECHNICAL ======-----

	Texture format: Each sky is a 6 sided cubemap. Source PNG texture resolution per-side ranges from x1024 to x2048. 

	Each sky has its own 6 sided skybox material which you can set to your scene's current skybox. 
	Please consult the Unity documentation if you are unsure how to do this.
	http://docs.unity3d.com/Manual/HOWTO-UseSkybox.html
	They are set as /mobile/skyboxes shaders - which should be fastest - but you can change them to the other 6sided skybox shaders that ship with Unity.
	Some add tint, exposure and rotation controls.

	Import Settings: Textures are initially imported at low resolution to hasten download and import speed, and also reduce initial disk usage.
	To increase the size of the imported texture raise the resolution settings on the inspector when you have the sky textures selected.

	Skies are sorted by time of day or style in folders.
	 
	Each sky has a folder which contains the textures and a material with those textures assigned.


-----====== DEMO SCENE ======-----

	Each sky folder also has a demo scene. This shows a simple low-poly environment to demonstrate lighting and fog settings for that sky.  

	It was originally lit in the Deferred Lighting Rendering Path with Linear lighting Color Space. 
	For intended values you will need a project with those settings.
	(Under Edit->Project Settings->Player)
	If you have to change these settings it may be necessary to re-import the sky textures.

	The environment uses a simple diffuse & specular shader which utilises vertex colours for ambient occlusion and dynamic snow applied by height. ( Shader created using ShaderForge: http://u3d.as/content/joachim-holm-r/shader-forge/6cc )

	The demo scene can benefit from increasing the Pixel light count in quality settings, and the Shadow Distance.


-----====== BY ======-----

	This asset pack is by Richard Whitelock.
	A game developer, digital artist & photographer.
	15+ years in the games industry working in a variety of senior art roles on 20+ titles. 
	Particularly experienced in environment art, lighting & special FX.
	Currently working on various indie game & personal projects. 

	http://www.richardwhitelock.com

	http://www.twitter.com/rpgwhitelock/

