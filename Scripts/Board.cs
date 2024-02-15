using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    // reference to the boards tilemap
    public Tilemap tilemap { get; private set; }

    //refernce to the current piece in play
    public Piece activePiece { get; private set; }

    //Array of each possible tetromino
    public TetrominoData[] tetrominoes;

    //2D vector of the boards width and height (x,y)
    public Vector2Int boardSize = new Vector2Int(10, 20);

    //3D vector of a pieces spawn position
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);


    //keep game score
    public int score = 0; 
    //display current score
    public Text scoreText;
    //keep number of lines that are cleared consecutively 
    private int linesCleared;

    //private but available to inspector, holds the
    //transform/position where the next tetromino will be placed
    [SerializeField] private Transform nextPiecePosition;
    //hold the converted position of a transform to vector3
    public Vector3Int previewPosition;
    
    //reference to the next piece that will be used
    public Piece nextPiece { get; private set; }


    //audio
    //refernce to the audio source in the board object
    public new AudioSource audio;
    //Array that holds all the audio clips that the board object will play
    public AudioClip[] soundClips;

    
    //keep track of the current stage
    public int stage;
   
    //Property that returns a RectInt (datafield, with a getter/setter created in place)
    public RectInt Bounds
    {
       //Creating the get method
       get
        {
            //finds the position of the corner of the rectangle, taking half the board x and y (starts at 0,0:
            //i.e the center of the screen)
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            //creates the rectangle, taking a position, and the size of the board
            return new RectInt(position, boardSize);
        }
    }

    //Start of the game
    private void Awake()
    {
        //get tilemap and Piece references from the board gameobject
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>(); 


        //create the next tetromino piece that will be played
        this.nextPiece = this.gameObject.AddComponent<Piece>();
        //disable to keep it from interacting with other things
        this.nextPiece.enabled = false;

        //convert the transform to a vector to have position of where the next piece will be displayed
        this.previewPosition = new Vector3Int((int)nextPiecePosition.position.x, (int)nextPiecePosition.position.y, 0);
        
        //start the stages
        this.stage = 1;
        

        //loop through the array of tetrominoes and give each piece its set of data
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }


    //method called at beginning of the game
    private void Start()
    {
        //call method to create the first piece
        SetNextPiece();
        //call method that spawns in the first piece
        SpawnPiece();
    }

    //method that creates and sets upcoming pieces
    private void SetNextPiece()
    {
        //clear previous piece shown
        if (this.nextPiece.cells != null)
        {
            Clear(this.nextPiece);
        }

        //randomly choose a number from 0, length of the tetromonio array
        int random = Random.Range(0, this.tetrominoes.Length);
        //set the data for the randomly chosen data
        TetrominoData data = this.tetrominoes[random];


        //call method to initialize the piece, (would allow it to move but
        //nextPiece is disabled)
        this.nextPiece.Initialize(this, this.previewPosition, data);
        //sets the piece on the tilemap
        Set(this.nextPiece);
    }


    //
    public void SpawnPiece()
    {

        //calls method to initialize piece; passes board state, the position it spawns, and the type of tetromino it is
        this.activePiece.Initialize(this, spawnPosition, this.nextPiece.data);

        //checks if a piece that has just spawned is in a valid position (isn't colliding with another tetromino)
        if (!IsValidPosition(this.activePiece, this.spawnPosition))
        {
            //not a valid position, game ends
            GameOver();
        } 
        else
        {
            //is in a valid position, set it on the tilemap
            Set(this.activePiece);
        }


        //call method to create the next tetromino that will be used
        SetNextPiece();

        //calls method to check the score and set the stage
        SetStage();
    }


    //method thats called when a newly spawned piece isn't in a valid position 
    private void GameOver()
    {
        //clears all of the tiles on the tile map
        this.tilemap.ClearAllTiles();
        //resets score and the stage
        this.score = 0;
        this.stage = 1;

        //resets the speed of the tetromino picece
        this.activePiece.stepDelay = 0.8f;

    }

    //method that sets a tetromino on the tilemap
    public void Set(Piece piece)
    {

        //loops through a pieces cells, and places them one by one
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //takes the current position that the piece in general is +
            //the position that an individual cell is in 
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            //sets tile on the map at a position and uses the corresponding tile
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
 
    }


    //Same as Set(Piece piece) but instead makes that position null/empty
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    //method that checks if a piece is in a valid position
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        //copy to another variable so more readable if (!this.Bounds.Contains((Vector2Int)tilePosition))
        RectInt bounds = this.Bounds;
        

        // loops through a pieces cells, position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            
            //the position of where the cell would be placed if its valid
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        //passed the checks to it is true
        return true;
    }


    //method that clears out lines when it is filled up
    public void ClearLines()
    {
        //get the game bounds
        RectInt bounds = this.Bounds;
        //starting from the bottom of the bounds (going up) 
        int row = bounds.yMin;
        //start counting how many lines get cleared consecutively
        this.linesCleared = 0;

        //loop through each row until reach the top
        while (row <bounds.yMax)
        {
            //check if the current row if full
            if (IsLineFull(row))
            {
                //line is full, call method to clear it
                LineClear(row);
                //add 1 for every line thats cleared simultaneously
                this.linesCleared +=  1;
            }
            //move on the next row
            else
            {
                row++;
            }
        }

        //call method to set the score for each line cleared
        SetScore();
        //reset the count for the next call
        this.linesCleared = 0;
    }


    //method that checks if a tilemap row if full
    private bool IsLineFull(int row)
    {
        //get the game bounds
        RectInt bounds = this.Bounds;

        //loop through the lines *cell*
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            //cells position that is being checked at col(x), row(y)
            Vector3Int position = new Vector3Int(col, row, 0);

            //checks if the position chosen is empty
            if (!this.tilemap.HasTile(position))
            {
                //line is not full
                return false;
            }
        }

        //line is full
        return true;
    }


    //method that clears and moves down the next lines
    private void LineClear(int row)
    {
        //get the game bounds
        RectInt bounds = this.Bounds;

        //loop through the lines *cell*
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            //cells position that is being checked at col(x), row(y)
            Vector3Int position = new Vector3Int(col, row, 0);
            //set that position null/empty
            this.tilemap.SetTile(position, null);
        }

        //set the audio source to be a clip in soundClips
        audio.clip = soundClips[0];
        //plays the sound
        audio.Play();


        //move the rows down

        //loop from the row that was just emptied
        while (row < bounds.yMax)
        {
            //loop through the lines *cell*
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                //cells position that is being checked at col(x), row(y)+1 (row above empty one) 
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                //get the tile that is in the cells position
                TileBase above = this.tilemap.GetTile(position);

                //reset position to be on the row
                position = new Vector3Int(col, row, 0);
                //set the the tile above to the current position
                this.tilemap.SetTile(position, above);
            }

            //move on to  the next row above and repeat
            row++;
        }

    }


    //method that sets the score 
    private void SetScore()
    {

        //checks the amount of lines that have been cleared  and gives points accordingly 
        if (this.linesCleared == 1)
        {
            this.score += 40;
        }
        else if (this.linesCleared == 2)
        {
            this.score += 100;
        }
        else if (this.linesCleared == 3)
        {
            this.score += 300;

        } 
        else if(this.linesCleared == 4)
        {
            this.score += 1200;
        }

        //updates the score in thte UI
        this.scoreText.text = this.score.ToString().PadLeft(2, '0');
    }

    //method that gets the score
    public int GetScore(){
        return this.score;
    }


    //method that progresses the stages and makes the game harder
    private void SetStage()
    {

        //checks the amount of points the player has, and increases the drop speed accordingly
        if (this.GetScore() >= 13500 && this.stage == 4)
        {
            this.activePiece.stepDelay = 0.2f;

        }
        else if (this.GetScore() >= 8000 && this.stage == 3)
        {
            this.activePiece.stepDelay = 0.4f;
            this.stage = 4;
        }
        else if (this.GetScore() >= 4000 && this.stage == 2)
        {
            this.activePiece.stepDelay = 0.6f;
            this.stage = 3;
        }
        else if (this.GetScore() >= 1000 && this.stage == 1)
        {
            this.activePiece.stepDelay = 0.7f;
            this.stage = 2;
        }
    }
        
}
