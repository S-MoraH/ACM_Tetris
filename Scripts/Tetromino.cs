using UnityEngine;
using UnityEngine.Tilemaps;


// an enumerated list of values (constants (unchangeable/read-only variables))
//list each block (tetromino) for the game
public enum Tetromino
{
    I, J, L, O, S, T, Z
}


//Set the data for each tetromino
[System.Serializable]
public struct TetrominoData
{
    //takes a tile (tile/image used in game)
    public Tile tile;
    //which tetromino from the enum that it is (ID of it)
    public Tetromino tetromino;

    //array to hold the tile positions that make up an entire tetromino piece
    public Vector2Int[] cells { get; private set; }

    //2D array that holds the tests needed if roatating the piece fails/succeeds
    public Vector2Int[,] wallKicks { get; private set; }

    //method gets and sets the data for a tetromino
    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino];
        this.wallKicks = Data.WallKicks[this.tetromino];
    }

}