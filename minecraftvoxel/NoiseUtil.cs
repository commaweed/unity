using UnityEngine;
using System.Collections;

/// <summary>
/// Noise generating function utility that can be used to shape the minecraft terrain.
/// </summary>
public class NoiseUtil {

    /// <summary>
    /// Represents the maximum height for the terrain; anything above that up to the maximum chunk height will be AIR.
    /// Note the maximum chunk height is defined by:  MyWorld.CHUNK_COLUMN_SIZE * MyWorld.CHUNK_SIZE
    /// </summary>
    public static int MAX_TERRAIN_HEIGHT = (int) (MyWorld.CHUNK_COLUMN_SIZE * MyWorld.CHUNK_SIZE * 0.6f);

    private static float smooth = 0.01f;
    private static int octaves = 4;
    private static float persistence = 0.5f;

    // TODO: make blocktype a powerful enum or object 
    private static int GetBlockMaxHeight(BlockType blockType) {
        switch (blockType) {
            case BlockType.DIRT:
                return (int) (MAX_TERRAIN_HEIGHT * 0.9f);
            case BlockType.STONE:
                return (int) (MAX_TERRAIN_HEIGHT * 0.7f);
            case BlockType.RED_STONE:
                return 20;
            case BlockType.DIAMOND:
                return 16;
            case BlockType.BED_ROCK:
                return 0;
            default:
                return MAX_TERRAIN_HEIGHT;
        }
    }

    /// <summary>
    /// Returns the block type at the given world position according to the perlin height function value.
    /// </summary>
    /// <param name="worldPosition">The position for which to return the block type using the noise function.</param>
    /// <returns>The block type that should be rendered to the given world position.</returns>
    public static BlockType GetBlockAt(Vector3 worldPosition) {
        BlockType result;

        if (worldPosition.y <= GetBlockMaxHeight(BlockType.BED_ROCK)) {
            result = BlockType.BED_ROCK;
        } else if (worldPosition.y <= GenerateHeight(worldPosition, BlockType.STONE)) {
            result = BlockType.STONE;

            if (worldPosition.y <= GetBlockMaxHeight(BlockType.RED_STONE) && FractalBrownianMotion3d(worldPosition, 0.03f, 3) < 0.41f) {
                result = BlockType.RED_STONE;
            }

            if (worldPosition.y <= GetBlockMaxHeight(BlockType.DIAMOND) && FractalBrownianMotion3d(worldPosition, 0.01f, 2) < 0.42f) {
                result = BlockType.DIAMOND;
            }
        } else if (worldPosition.y <= GenerateHeight(worldPosition, BlockType.DIRT)) {
            result = BlockType.DIRT;
        } else if (worldPosition.y <= GenerateHeight(worldPosition, BlockType.GRASS)) {
            result = BlockType.GRASS;
        } else {
            result = BlockType.AIR;
        }

        // create caves
        if (worldPosition.y > 0 && worldPosition.y < MAX_TERRAIN_HEIGHT && FractalBrownianMotion3d(worldPosition, 0.1f, 3) < 0.42f) {
            result = BlockType.AIR; 
        }

        return result;
    }

    /// <summary>
    /// Use noise function to calculate height (i.e. y) at the given (x,z) coordinate.
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="z">The z cooridnate</param>
    /// <returns>The y coordinate according to the underlying perlin noise function.</returns>
    private static int GenerateHeight(Vector3 worldPosition, BlockType blockType) {
        float height = Map(
            0, 
            GetBlockMaxHeight(blockType),
            0, 
            1, 
            FractalBrownianMotion(worldPosition.x * smooth, worldPosition.z * smooth, octaves, persistence)
        );
        return (int) height;
    }

    /// <summary>
    /// Transforms grid 0-1 to 0-255.
    /// </summary>
    /// <param name="newMin"></param>
    /// <param name="newMax"></param>
    /// <param name="oldMin">Minimum value of the old grid.</param>
    /// <param name="oldMax">Maximum value of the old grid.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static float Map(
        float newMin,
        float newMax,
        float oldMin,
        float oldMax,
        float value
    ) {
        return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(oldMin, oldMax, value));
    }

    /// <summary>
    /// Uses noise to generate the height value of the top terrain.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="octaves"></param>
    /// <param name="persistence"></param>
    /// <returns></returns>
    private static float FractalBrownianMotion(float x, float z, float octaves, float persistence) {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        float offset = 32000f;
        for (int i = 0; i < octaves; i++) {
            total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }
        return total / maxValue;
    }

    /// <summary>
    /// Uses noise to generate the height value of caves.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    private static float FractalBrownianMotion3d(Vector3 worldPosition, float smooth, int octaves) {
        float xy = FractalBrownianMotion(worldPosition.x * smooth, worldPosition.y * smooth, octaves, persistence);
        float yz = FractalBrownianMotion(worldPosition.y * smooth, worldPosition.z * smooth, octaves, persistence);
        float xz = FractalBrownianMotion(worldPosition.x * smooth, worldPosition.z * smooth, octaves, persistence);
        float yx = FractalBrownianMotion(worldPosition.y * smooth, worldPosition.x * smooth, octaves, persistence);
        float zy = FractalBrownianMotion(worldPosition.z * smooth, worldPosition.y * smooth, octaves, persistence);
        float zx = FractalBrownianMotion(worldPosition.z * smooth, worldPosition.x * smooth, octaves, persistence);
        return (xy + yz + xz + yx + zy + zx) / 6.0f;
    }

}
