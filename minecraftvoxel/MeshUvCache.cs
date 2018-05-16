
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents a cache of UV arrays based upon the underlying blockAtlasImage.  This image is really a 16x16 grid a minecraft
/// textures.
/// </summary>
public class MeshUvCache : MonoBehaviour {

    #region Singleton
    public static MeshUvCache Instance;
    public void Awake() {
        buildCache();
        Instance = this;
    }
    #endregion 

    [SerializeField]
    private Texture2D blockAtlasImage;

    // the blockatlas minecraft image is a 16x16 grid 
    public static readonly int MAX_IMAGE_GRID_WIDTH = 16;

    /// <summary>
    /// The maximum linear index value that is used in the cache.
    /// </summary>
    public static readonly int MAX_INDEX = MAX_IMAGE_GRID_WIDTH * MAX_IMAGE_GRID_WIDTH;

    /** 
     * The UV Array cache will hold all of the minecraft image grid mesh UV arrays (length = 4 vertices).  The
     * key represents the row x column index = (row, column).  Thus, assuming the grid is 16x16:
     * 
     * 0 = (0, 0), 1 = (0, 1), ... , 15 = (0, 15)
     * 16 = (1, 0), 17 = (1, 1), ..., 31 = (1, 15)
     * ...
     * 240 = (15, 0), 241 = (15, 1), ..., 255 = (15, 15)
     * 
     * (0, 0) = the bottom left corner of the image
     * (15, 15) = the top right corner of the image
     */
    private Dictionary<int, Vector2[]> minecraftImageMeshUvCache = new Dictionary<int, Vector2[]>();

    /// <summary>
    /// Creates the minecraftImageMeshUvCache by processing the underlying blockAtlasImage.  Note, position (0,0) refers to the bottom
    /// left corner of the image grid.
    /// </summary>
    private void buildCache() {
        // Add all of the blockatlas UV array vectors to the minecraftImageMeshUvCache
        if (this.blockAtlasImage == null) {
            throw new System.InvalidOperationException("Illegal State; the blockAtlasImage has not been added to the script!");
        }

        int sw = blockAtlasImage.width / MAX_IMAGE_GRID_WIDTH;
        int sh = blockAtlasImage.height / MAX_IMAGE_GRID_WIDTH;

        for (int row = 0; row < MAX_IMAGE_GRID_WIDTH; row++) {
            for (int column = 0; column < MAX_IMAGE_GRID_WIDTH; column++) {
                float uv1x = (column * sw) / (float)blockAtlasImage.width;
                float uv1y = (row * sh) / (float)blockAtlasImage.height;
                float uv2x = (column * sw + sw) / (float)blockAtlasImage.width;
                float uv2y = (row * sh) / (float)blockAtlasImage.height;
                float uv3x = (column * sw) / (float)blockAtlasImage.width;
                float uv3y = (row * sh + sh) / (float)blockAtlasImage.height;
                float uv4x = (column * sw + sw) / (float)blockAtlasImage.width;
                float uv4y = (row * sh + sh) / (float)blockAtlasImage.height;

                int index = computeIndex(row, column);
                Vector2[] uvs = new Vector2[] {
                    new Vector2(uv4x, uv4y),
                    new Vector2(uv3x, uv3y),
                    new Vector2(uv1x, uv1y),
                    new Vector2(uv2x, uv2y)
                };

                this.minecraftImageMeshUvCache.Add(index, uvs);
            }
        }
    }

    /// <summary>
    /// Utility that will convert the given row and column in the minecraft blockatlas image grid into a linear value 
    /// that is used in the cache (i.e. a value from 0 to MAX_INDEX - 1)
    /// </summary>
    /// <param name="row">The row in the grid.  Row 0 starts at the bottom of the blockatlas minecraft image grid.</param>
    /// <param name="column">The column in the grid.  Column 0 starts at the left side of the blockatlas minecraft image grid.</param>
    /// <returns></returns>
    private static int computeIndex(int row, int column) {
        return (row * MAX_IMAGE_GRID_WIDTH) + column;
    }

    /// <summary>
    /// Returns the UV Array at the given index.
    /// </summary>
    /// <param name="index">The index in the cache where 0 represents (0,0) in the image grid and is the bottom left-corner.</param>
    /// <returns>The Mesh UV Array values as a Vector2[] at the given index location.</returns>
    public Vector2[] GetUvArray(int index) {
        if (index < 0 || index > MAX_INDEX) {
            throw new System.ArgumentException(string.Format("Invalid index; it must be in range [0,{0}]", MAX_INDEX));
        }

        return this.minecraftImageMeshUvCache[index];
    }

    /// <summary>
    /// Returns the UV Array for the given blockType and cubeSide.
    /// </summary>
    /// <param name="blockType">The block type.</param>
    /// <param name="cubeSide">The block side.</param>
    /// <returns>The Mesh UV Array values as a Vector2[] for the given block and side.</returns>
    public Vector2[] GetUvArray(BlockType blockType, CubeSide cubeSide) {
        int index;
        switch (blockType) {
            case BlockType.STONE:
                index = (int) ImageBlockIndex.STONE;
                break;
            case BlockType.DIRT:
                index = (int) ImageBlockIndex.DIRT;
                break;
            case BlockType.GRASS:
                switch (cubeSide) {
                    case CubeSide.TOP:
                        index = (int) ImageBlockIndex.GRASS_TOP;
                        break;
                    case CubeSide.BOTTOM:
                        index = (int) ImageBlockIndex.DIRT;
                        break;
                    default:
                        index = (int) ImageBlockIndex.GRASS_SIDE;
                        break;
                }
                break;
            case BlockType.DIAMOND:
                index = (int) ImageBlockIndex.DIAMOND;
                break;
            case BlockType.RED_STONE:
                index = (int) ImageBlockIndex.RED_STONE;
                break;
            case BlockType.BED_ROCK:
                index = (int) ImageBlockIndex.BED_ROCK;
                break;
            default:
                throw new ArgumentException("Invalid block type [" + blockType + "]; it has not been configured for UVArrays");
        }

        return GetUvArray(index);
    }

    // used internally to display debug information regarding the block atlas image grid
    private void DisplayDebug() {
        // display values
        for (int i = 0; i < MAX_INDEX; i++) {
            Vector2[] uvs = GetUvArray(i);
            print(string.Format(
                "{0} - ({5},{6}): [{1},{2},{3},{4}]",
                i,
                uvs[0].ToString("0.0000"),
                uvs[1].ToString("0.0000"),
                uvs[2].ToString("0.0000"),
                uvs[3].ToString("0.0000"),
                i / MAX_IMAGE_GRID_WIDTH,
                i % MAX_IMAGE_GRID_WIDTH
            ));
        }

        // display grid
        StringBuilder grid = new StringBuilder();
        for (int row = MAX_IMAGE_GRID_WIDTH - 1; row >= 0; row--) {     
            for (int column = 0; column < MAX_IMAGE_GRID_WIDTH; column++) {
                grid.Append(string.Format("({0,2},{1,2}):{2,3} ", row, column, computeIndex(row, column)));
            }
            grid.Append("\n");
        }
        print(grid.ToString());
    }

    public void Start() {
        //DisplayDebug();
    }
}