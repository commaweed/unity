using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a minecraft chunk as a cube with maximum size=16.
/// </summary>
public class MyChunk {

    private GameObject chunkGameObject;
    private int chunkSize;
    private MyBlock[,,] chunkMetadata;
    private Material minecraftMaterial;


    /// <summary>
    /// Initialize with the given values.
    /// </summary>
    /// <param name="size">The size to use for x, y, and z.</param>
    /// <param name="position">The origin position of the chunk.</param>
    /// <param name="minecraftMaterial">The blockatlas image that has all the minecraft textures.</param>
    public MyChunk(
        int chunkSize,
        Vector3 position, 
        Material minecraftMaterial
    ) {
        if (chunkSize < 1 || chunkSize > 16) {
            throw new System.ArgumentException("Invalid chunkSize; it must be between 1 and 16 inclusive!");
        }
        if (position == null) {
            throw new System.ArgumentException("Invalid position; it cannot be null!");
        }
        if (minecraftMaterial == null) {
            throw new System.ArgumentException("Invalid minecraftMaterial; it cannot be null!");
        }

        this.chunkSize = chunkSize;
        this.minecraftMaterial = minecraftMaterial;

        this.chunkGameObject = new GameObject(CreateChunkName(position));
        this.chunkGameObject.transform.position = position;

        chunkMetadata = createChunkMetadata();    
    }

    /// <summary>
    /// Create the chunk name based upon the given position.
    /// </summary>
    /// <param name="position">The origin position of the chunk.</param>
    /// <returns>A string representing the name of this chunk.</returns>
    public static String CreateChunkName(Vector3 position) {
        return string.Format(
            "{0}_{1}_{2}",
            (int)position.x,
            (int)position.y,
            (int)position.z
        );
    }

    /// <summary>
    /// Creates the chunk metadata by constructing the unrealized blocks.  They will later be realized (i.e. Meshes will be
    /// created).
    /// </summary>
    /// <returns></returns>
    private MyBlock[,,] createChunkMetadata() {
        MyBlock[,,] chunkMetadata = new MyBlock[chunkSize, chunkSize, chunkSize];
        for (int z = 0; z < chunkSize; z++) {
            for (int y = 0; y < chunkSize; y++) {
                for (int x = 0; x < chunkSize; x++) {
                    Vector3 position = new Vector3(x, y, z);
                    Vector3 worldPosition = position + chunkGameObject.transform.position;
                    chunkMetadata[x, y, z] = new MyBlock(
                        NoiseUtil.GetBlockAt(worldPosition), 
                        position, 
                        chunkGameObject.gameObject
                    );
                }
            }
        }
        return chunkMetadata;
    }

    /// <summary>
    /// Return a reference to the chunk's GameObject.
    /// </summary>
    public GameObject ChunkGameObject {
        get { return this.chunkGameObject; }
    }

    /// <summary>
    /// Return the current chunk size that is being used for this chunk. 
    /// </summary>
    public int ChunkSize {
        get { return this.chunkSize; }
    }

    /// <summary>
    /// Return a reference to this chunk's metadata.
    /// </summary>
    public MyBlock[,,] ChunkMetadata {
        get { return this.chunkMetadata;  }
    }

    /// <summary>
    /// Returns the name of this chunk; the name represents the global origin position of the chunk.
    /// </summary>
    /// <returns>The name of the chunk.</returns>
    public override string ToString() {
        return this.chunkGameObject.name;
    }
}
