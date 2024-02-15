using UnityEngine;

public class Piece : MonoBehaviour
{
    //reference to the board
    public Board board { get; private set; }

    //reference to what type of tetromino it is
    public TetrominoData data { get; private set; }
    //Array that holds a copy of a pieces cells
    public Vector3Int[] cells { get; private set; }

    //reference to the position of the piece
    public Vector3Int position { get; private set; }
    //holds the index piece currently is on when rotating
    public int rotationIndex { get; private set; }

    //hold how fast it steps down
    public float stepDelay = 1f;
    //holds how long it takes for a piece
    //to lock in if its been stationary
    public float lockDelay = 0.5f;

    //holds the time that has elapsed since
    //the last step 
    private float stepTime;
    //holds the time that has elapsed since
    //the last its moved 
    private float lockTime;

    

    //method that initializes all the pieces data
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;
        this.rotationIndex = 0;
        //start the step timer (pushing the time into the future)
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        
        
        //copy the cells array in data to an array in the piece
        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }


    //method thats called every frame
    private void Update()
    {
        //clears the previous drawn piece from the tilemap
        this.board.Clear(this);
        
        //increase the lock timer
        this.lockTime += Time.deltaTime;

        //takes in the key that the player enters to move piece
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //method call to rotate left
            Rotate(-1);
        } 
        else if (Input.GetKeyDown(KeyCode.E))
        {
            //method call to rotate right
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //method call to move left
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //method call to move right
            Move(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            //method call to move down
            Move(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //method call to drop piece fast
            HardDrop();
        }

        //checks if the current time exceeds the step timer
        //(time doesn't stop & will eventually pass the stepTime
        // that was set to be in the future)
        if (Time.time >= this.stepTime)
        {
            Step();
        }

        //redraws the piece onto the tilemap with the updated position/orientation
        this.board.Set(this);
    }

    //
    private void Step()
    {
        //push stepTime further into the future (reset stepTime to be further in
        // the future)
        this.stepTime = Time.time + this.stepDelay;
        
        //call the move method to move down
        Move(Vector2Int.down);

        //piece won't be able to go further down & the lock time will exceed
        //the lock delay
        if (this.lockTime >= this.lockDelay)
        {
            //call lock method
            Lock() ;
        }
    }

    //method that pushes the piece all the way to the bottom
    private void HardDrop()
    {
        // keep moving down until failure
        while (Move(Vector2Int.down))
        {
            continue;
        }

        //call lock method
        Lock();
    }

    //method to lock a piece in place
    private void Lock()
    {
        //draw the piece on the tilemap
        this.board.Set(this);

        //set the audio source to be a clip in soundClips
        this.board.audio.clip = this.board.soundClips[1];
        //plays the sound
        this.board.audio.Play();
        
        //calls the method to check after a piece has been placed
        this.board.ClearLines();
        //spawns in the next piece
        this.board.SpawnPiece();
    }


    //method that allows piece to move (takes the direction its moving
    private bool Move(Vector2Int translation)
    {
        //get copy the current position
        Vector3Int newPosition = this.position;
        //update it with where it is potentially moving
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        //check if the new position is possible
        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            //make current position equal the now tested possible position
            this.position = newPosition;

            //everytime the piece moves reset the lock timer
            this.lockTime = 0f;
        }

        //return if move was successful/failure
        return valid;
    }


    //method that rotates the current piece, takes the direction it will rotate too
    private void Rotate (int direction)
    {
        //keep the orignal index incase of failure
        int originalRotation = this.rotationIndex;
        //Update the index with the new possible rotation
        this.rotationIndex  = Wrap(this.rotationIndex + direction, 0, 4);

        //call method to rotate the piece
        ApplyRotationMatrix(direction);

           //check if the rotation failed the tests
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            //reset the index back
            this.rotationIndex = originalRotation;
            //rotate the piece back the other way
            ApplyRotationMatrix(-direction);
        }
    }

    //method that rotates the current piece
    private void ApplyRotationMatrix(int direction)
    {
        //loops through each cell in the tetromino
        for (int i = 0; i < this.cells.Length; i++)
        {
            //get the individual cell being worked on
            Vector3 cell = this.cells[i];

            //x and y of the new cell
            int x, y;

            //check which tetromino it is and rotate it
            switch (this.data.tetromino)
            {
                //I & O are special cases
                case Tetromino.I:
                case Tetromino.O:
                    //needs to be .5 off
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    //current cells x * cos * direction of rotation + current cells y * sin * direction of rotation 
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));     
                    //current cells x * -sin * direction of rotation + current cells y * cos * direction of rotation                        //rounded to the highest int
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                default:
                    //current cells x * cos * direction of rotation + current cells y * sin * direction of rotation 
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    //current cells x * -sin * direction of rotation + current cells y * cos * direction of rotation                       //rounded to the closest int
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            //set the cells new position
            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }


    //method that tests a piece when it rotates, takes the current rotation index and direction of the possible rotation
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        //calls method to get the index for the wallkick
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        //loop that goes through each test
        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            //get the potential position thats going to be tested
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i]; //takes [state currently in, start first test (first position tested)

            //check if the test/potential rotation of the piece can be moved(is valid to move) 
            if (Move(translation))
            {
                //was able to move/passed test
                return true;
            }
        }

        //failed thet test
        return false;
    }

    //method that gets the current state/index of the piece for the wallkick 
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        //multiply current index by 2
        int wallKickIndex = rotationIndex * 2;

        // check if the direction that will happen is negative (moving left)
        if (rotationDirection < 0)
        {
            //subtract one of its going left
            wallKickIndex--;
        }

        //call method to wrap index back around to the start or the end if needed
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    //standard method that wraps an array back to the start(min) or the end(max)
    private int Wrap(int input, int min, int max)
    {
        //read it, you got that
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }



}