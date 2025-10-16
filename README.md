<p align="center">
  <img src="https://raw.githubusercontent.com/deadwronggames/ZSharedAssets/main/Banner_Zombie.jpg" alt="ZCommon Banner" style="width: 100%; max-width: 1200px; height: auto;">
</p>

# DeadWrongGames ZUtils

A collection of static utility classes and extension classes for Unity projects, designed to simplify common tasks and improve workflow efficiency. These utilities cover areas ranging from mesh handling to randomization, UI, tweening, and string manipulation.


## Overview

The ZUtils library provides the following major areas of functionality:

- Core Utilities (ZMethods), e.g.:<br>
General-purpose helpers for actions, lazy initialization, float/double comparisons, and safe casting. Utilities for working with enums and dictionaries. Methods for working with 2D arrays and indices, neighbor calculations, and safe numeric operations.

- Audio Utilities (ZMethodsAudio), e.g.:<br>
Manage audio clips and sources, including random selection avoiding recently played clips. Query remaining audio clip time and facilitate audio management.

- Async Utilities (ZMethodsCoroutines), e.g.:<br>
Delay actions by frames or time, repeated actions, and cached WaitForSeconds/WaitForSecondsRealtime. Safe stopping of coroutines. TODO extend with Tasks and UniTasks.

- Crypto Utilities (ZMethodsCrypto), e.g.:<br>
Encrypt and decrypt strings using AES. Manage key/IV generation and persistence using ScriptableObjects. Provides secure and convenient encryption handling for e.g. save files.

- Mesh Utilities (ZMethodsMeshes), e.g.:<br>
Combine skinned and unskinned meshes into a single SkinnedMeshRenderer

- Position Utilities (ZMethodsPosition), e.g.:<br>
Generate random positions in configurable patterns, such as spirals or squares.

- RNG Utilities (ZMethodsRandom), e.g.:<br>
Random selection, shuffling, and weighted random choices, Gaussian, log-normal distributions, probabilistic calculations, coin flips, and vector randomization.

- String Utilities (ZMethodsString), e.g.:<br>
Formatting, colorization, resizing, and other UI-friendly manipulations. Converting integers to Roman numerals and replace numbers in strings. Handle escaped characters for Unity inspector display.

- DOTween Utilities (ZMethodsTween), e.g.:<br>
Recursive tween management and handy UI animation wrappers.

- Unity Helpers (ZMethodsUnity), e.g.:<br>
Transform and GameObject operations (traverse children, destroy children, recursive layer assignment). RectTransform helpers for size, stretch, and actual size calculations. Color extensions and safe null checks for Unity objects.


## Design Highlights

- Consistent use of extension methods for cleaner syntax.
- Edge-case safe operations for Unity-specific objects like meshes, transforms, and UI elements.
- Modular and easy to integrate into existing Unity projects.
- Designed with performance considerations, e.g., using cached arrays, avoiding unnecessary allocations.


## Installation
- Install via Unity Package Manager using the Git URL: https://github.com/deadwronggames/ZUtils
- Include in your code (when needed) via the namespace: 
```csharp 
using DeadWrongGames.ZUtils;
```

## Example Usage
```csharp 
using DeadWrongGames.ZUtils;
using UnityEngine;

// Safe casting and easy console printing
public void OnEventWithParameter(object parameterObject)
{
    if (!ZMethods.TryCast(parameterObject, out TParameter parameter, verbose: false)) return;
    
    "Successful cast".Print();
}

// Delayed action
ZMethodsCoroutines.DelayedAction(this, delay: 5f, () => "5 Seconds later...".Print());

// Combine all meshes under a parent GameObject
ZMethodsMeshes.CombineAllMeshes(parentGameObject);

// Colorize a string for UI
string coloredText = "Hello World".ColorizeString(Color.red);
```

## Notes
- **Work in progress**, some functionality may change.