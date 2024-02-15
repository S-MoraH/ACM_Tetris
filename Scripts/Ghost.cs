using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    //reference to the tile that will be used
    public Tile tile;
    //refernce to the board
    public Board board;
    //refernce to the piece that it current in play
    public Piece trackingPiece;

    //refernce to its tilmap
    public Tilemap tilemap { get; private set; }

    //copy of the current pieces cells
    public Vector3Int[] cells { get; private set; }
    
    //reference to the position of the piece
    public Vector3Int position { get; private set; }


    //called when the game starts
    private void Awake()
    {
        //get refernce to tilemap
        this.tilemap = GetComponentInChildren<Tilemap>();
        //instantiate the cells array
        this.cells = new Vector3Int[4];
    }

    //method that is called last after all the other updates
    private void LateUpdate()
    {
        //calls to methods in order
        Clear();
        Copy();
        Drop();
        Set();
    }


    //method that clears the previous piece
    private void Clear()
    {
        //for each cell in the array
        for (int i = 0; i < this.cells.Length; i++)
        {
            //get the cells position
            Vector3Int tilePosition = this.cells[i] + this.position;
            //set the cell at the position to null/empty
            this.tilemap.SetTile(tilePosition, null);
        }
    }


    //method that copies the cells from the current piece to cells array
    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }


    //method that drops the ghost piece down
    private void Drop()
    {
        //get the position of the current piece
        Vector3Int position = this.trackingPiece.position;

        //get the pieces current y (current row its on)
        int current = position.y;
        //get the bottom/the last row of the board,
        //-1 to push it one lower
        int bottom = -this.board.boardSize.y / 2 - 1;

        //clears the piece before testing
        //(would cause IsValidPosition to always be false
        //piece is starting in same position as ghost
        //, so take it off)
        this.board.Clear(this.trackingPiece);

        //loop, going from current row down to the bottom (up to down)
        for(int row = current; row >= bottom; row--)
        {
            
            //move the current position to the new position
            position.y = row;

            //check if the new position is in a valid position
            if(this.board.IsValidPosition(this.trackingPiece, position))
            {
                //its valid so set ghost piece to the new position
                this.position = position;
            }
            //stop for loop, it can't go any further
            else
            {
                break;
            }
        }

        //reset the tracking piece that was cleared earlier in the same position
        this.board.Set(this.trackingPiece);
    }


    //method that set the ghost piece to the tilemap
    private void Set()
    {
        //loops through the cells and places them
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }


}
