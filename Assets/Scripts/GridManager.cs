using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //For use button
using System.IO;
using System.Threading.Tasks;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    public AudioSource song;
    public GameObject ground;
    public Material white, Green, Red, Blue;
    public GameObject Robot;
    public int[,] Grid;
    public float[,] Qmatris;
    public int whiteReward, redReward, greenReward;
    
    int Vertical, Horizontal, Columns, Rows;
    public List<Squares> squares;
    public Vector3 oldGreenPos;
    public Vector3 oldBuePos;
    public Vector3 GreenPos;
    public Button createObstaclesButton;
    public Button startGameButton;
    public Button confirmButton;
    public int[] xPos = { -29, -26, -23, -20, -17, -14, -11, -8, -5, -2, 1, 4, 7, 10, 13, 16, 19, 22, 25, 28 };
    public int[] yPos = { 29, 26, 23, 20, 17, 14, 11, 8, 5, 2, -1, -4, -7, -10, -13, -16, -19, -22, -25, -28 };
    public int robotX = 0, robotY = 0, reward = 0, rewardAtStep = 0, currentState = 0, nextState = 0;
    public bool done = false;
    public bool wrong = false;
    public float old_value = 0, next_value = 0;
    public int next_action;
    public float discountFactor = 0.9f;
    public float next_max;
    public List<Vector3> statePath;
    public List<Vector3> bestStatePath;
    public int rewardAtStatePath = 0;
    public int stepAtStatePath = 0;
    public int loop = 100;
    public float epsilon = 10f;



    public class Squares
    {
        public GameObject obj { get; set; }
        public Vector3 pos { get; set; } //position info
        //public bool empty;  //nothing selected(Red,white,Green)
        public Material colorName { get; set; } //Default is White
        
        public Squares(Vector3 pos, Material colorName, GameObject obj)
        {
            this.pos = pos;
            this.colorName = colorName;
            this.obj = obj;
        }

    }
    private void Awake()
    {
      
        squares = new List<Squares>();
        Vertical = (int)Camera.main.orthographicSize;
        Horizontal = Vertical * (Screen.width / Screen.height);
        
        Columns = 20;
        Rows = 20;
        Grid = new int[Columns, Rows];
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                Grid[i, j] = Random.Range(0, 10);
                spawnTile(i, j, Grid[i, j]);
            }
        }

        Qmatris = new float[400, 8];
       for(int i = 0; i < 400; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                Qmatris[i, j] = 0f;

            }
        }
      



    }
    void Start()
    {

        createObstaclesButton.interactable = false;
        confirmButton.interactable = false;
        startGameButton.interactable = false;

    }
    public void Update()
    {
        if(ClickManager.flag == 0)
        {
            squares.ForEach(delegate (Squares s)
            {

                if (oldBuePos == s.pos && oldBuePos != ClickManager.leftClick)
                {
                    s.colorName = white;
                    s.obj.gameObject.GetComponent<SpriteRenderer>().material = white;
                }

            });
            squares.ForEach(delegate (Squares s)
            {

                if (s.pos == ClickManager.leftClick)
                {
                    s.colorName = Blue;
                    s.obj.gameObject.GetComponent<SpriteRenderer>().material = Blue;
                    oldBuePos = ClickManager.leftClick;
                    Robot.transform.position = ClickManager.leftClick;

                }

            });


            squares.ForEach(delegate (Squares s)
                {
                    
                        if(oldGreenPos == s.pos && oldGreenPos != ClickManager.rightClick)
                        {
                            s.colorName = white;
                            s.obj.gameObject.GetComponent<SpriteRenderer>().material = white;
                        }
                                
                });
                squares.ForEach(delegate (Squares s)
                {

                    if (s.pos == ClickManager.rightClick)
                    {
                        s.colorName = Green;
                        s.obj.gameObject.GetComponent<SpriteRenderer>().material = Green;
                        oldGreenPos = ClickManager.rightClick;
                    }

                });
            if ( oldGreenPos != new Vector3(0,0,0))
            {
                createObstaclesButton.interactable = true;
            }
        }
        else if(ClickManager.flag == 1)
        {
            squares.ForEach(delegate (Squares s)
            {

                if (s.pos == ClickManager.leftClick)
                {
                    if(s.colorName == white)
                    {
                        s.colorName = Red;
                        s.obj.gameObject.GetComponent<SpriteRenderer>().material = Red;
                        ClickManager.leftClick = Robot.transform.position; //That changes its color for one time.
                    }
                    else if(s.colorName == Red)
                    {
                        s.colorName = white;
                        s.obj.gameObject.GetComponent<SpriteRenderer>().material = white;
                        ClickManager.leftClick = Robot.transform.position; //That changes its color for one time.
                    }
                    
                }

            });
        }
    }
    
    private void spawnTile(int x, int y, int value)
    {
        

        var g = Instantiate(ground, new Vector2(3f * y - (Vertical - 2f), (Horizontal - 2f) - x * 3), Quaternion.identity); // creates object
        g.name = ("X:" + x + "Y:" + y); // names object


        Squares sqr = new Squares(new Vector2(3f * y - (Vertical - 2f), (Horizontal - 2f) - x * 3), white, g);
        squares.Add(sqr);
       

    }



    public void createObstacles()
    {
        ClickManager.flag = 1;
        createObstaclesButton.interactable = false;
        int obstaclesCount = 0;

        while (obstaclesCount < 150)
        {
            int random = Random.Range(0, 400);
            if(squares[random].colorName == white)
            {
                squares[random].colorName = Red;
                squares[random].obj.gameObject.GetComponent<SpriteRenderer>().material = Red;
                obstaclesCount++;
            }
        }

        confirmButton.interactable = true;
    }

    public void confirmGame()
    {


        ClickManager.flag = 2;
        startGameButton.interactable = true;
        confirmButton.interactable = false;
        var fileName = "Obstacles.txt";

        var sr = File.CreateText(fileName);
        


        squares.ForEach(delegate (Squares s)
        {

            if (s.colorName == Red)
            {
                sr.WriteLine("("+s.obj.name + " " + s.colorName.name+")");
            }

        });
        sr.Close();

    }
    public void startGame()
    {
        song.Play();
        ClickManager.flag = 3;
        startGameButton.interactable = false;
        squares.ForEach(delegate (Squares s)
        {

            if (s.colorName == Green)
            {
                GreenPos = s.pos;
            }

        });


        for (int i = 0; i < 20; i++)
        {
            if (xPos[i] == Robot.transform.position.x)
            {
                robotX = i;

            }
            if (yPos[i] == Robot.transform.position.y)
            {
                robotY = i;

            }
        }

        currentState = findState(robotX, robotY);
        int action = chooseAction(currentState);
        StartCoroutine(move());
        


     

    }

    //Actions: right, left, up, down, cross right up, cross right down, cross left up, cross left down(0,1,2,3,4,5,6,7)
    public int findState(int x, int y)
    {
        int state = ((y + 1) * 20) - (20 - (x + 1));
        state -= 1;
        return state;
    }
   
    public int chooseAction(int state)
    {
        float value = Qmatris[state, 0];
        int action = 0;
        float randomAction = Random.Range(0f,100f);
        

        if (epsilon >= randomAction){
            action = Random.Range(0, 8);
        }
        
        else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (value < Qmatris[state, i])
                    {
                        action = i;
                        value = Qmatris[state, i];
                    }
                }
        }
        
        
        
        
            
        
        

        if(action == 0)//right
        {
            if(robotX + 1 > 19 || robotX + 1 < 0)
            {
                action = 1;//left
            }
        }
        else if(action == 1)//left
        {
            if (robotX - 1 > 19 || robotX - 1 < 0)
            {
                action = 0; //right
            }
        }
        else if(action == 2)//up
        {
            if(robotY - 1 > 19 || robotY - 1 < 0){
                
                action = 3;//down
            }
        }
        else if(action == 3)//down
        {
            if (robotY + 1 > 19 || robotY + 1 < 0)
            {
                action = 2;//up
            }
        }
        else if(action == 4)//cross right up
        {
            if (robotY - 1 > 19 || robotY - 1 < 0 || robotX + 1 > 19 || robotX + 1 < 0)
            {
                action = 5; //cross right down
            }
        }
        else if(action == 5) //cross right down
        {
            if (robotY + 1 > 19 || robotY + 1 < 0 || robotX + 1 > 19 || robotX + 1 < 0)
            {
                action = 4; //cross right up
            }
        }
        else if(action == 6) //cross left up
        {
            if (robotY - 1 > 19 || robotY - 1 < 0 || robotX - 1 > 19 || robotX - 1 < 0)
            {
                action = 7; //cross left down
            }
        }
        else if(action == 7)//cross left down
        {
            if (robotY + 1 > 19 || robotY + 1 < 0 || robotX - 1 > 19 || robotX - 1 < 0)
            {
                action = 6; //cross left up
            }
        }

        return action;
     
    }
    

    public void setReward()
    {
        squares.ForEach(delegate (Squares s)
        {

            if (Robot.transform.position == s.pos)
            {
                if (s.colorName == white || s.colorName == Blue)
                {
                    reward += whiteReward;
                    rewardAtStep = whiteReward;
                    rewardAtStatePath += whiteReward;
                }
                else if (s.colorName == Red)
                {
                    reward += redReward;
                    rewardAtStep = redReward;
                    rewardAtStatePath += redReward;
                    
                }
                else if (s.colorName == Green)
                {
                    reward += greenReward;
                    rewardAtStep = greenReward;
                    rewardAtStatePath += greenReward;
                }
            }

        });
        
    }
    public void robotMove(int action,int robotX,int robotY)
    {
        if (action == 0)//right
        {
            if (robotX + 1 < 20 && robotX + 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX + 1], yPos[robotY], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;
                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {

                        if (s.colorName == Red)
                        {
                            wrong = true;

                        }
                    }
                });
            }
            
        }
        else if (action == 1)//left
        {
            if (robotX - 1 < 20 && robotX - 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX - 1], yPos[robotY], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {

                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });
            }
        }
        else if (action == 2)//up
        {
            if (robotY - 1 < 20 && robotY - 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX], yPos[robotY - 1], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {
                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });

            }
        }
        else if (action == 3) //down
        {
            if (robotY + 1 < 20 && robotY + 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX], yPos[robotY + 1], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {
                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });
            }
        }
        else if (action == 4) //cross right up
        {
            if (robotY - 1 < 20 && robotY - 1 >= 0 && robotX + 1 < 20 && robotX + 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX + 1], yPos[robotY - 1], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {
                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });
            }
        }
        else if (action == 5) //cross right down
        {
            if (robotY + 1 < 20 && robotY + 1 >= 0 && robotX + 1 < 20 && robotX + 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX + 1], yPos[robotY + 1], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {
                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });
            }
        }
        else if (action == 6) //cross left up
        {
            if (robotY - 1 < 20 && robotY - 1 >= 0 && robotX - 1 < 20 && robotX - 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX - 1], yPos[robotY - 1], Robot.transform.position.z);
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {
                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });

            }
        }
        else if (action == 7) //cross left down
        {
            if (robotY + 1 < 20 && robotY + 1 >= 0 && robotX - 1 < 20 && robotX - 1 >= 0)
            {

                Robot.transform.position = new Vector3(xPos[robotX - 1], yPos[robotY + 1], Robot.transform.position.z);
               
                for (int i = 0; i < 20; i++)
                {
                    if (xPos[i] == Robot.transform.position.x)
                    {
                        this.robotX = i;
                    }
                    if (yPos[i] == Robot.transform.position.y)
                    {
                        this.robotY = i;
                    }
                }
                stepAtStatePath += 1;

                setReward();
                nextState = findState(this.robotX, this.robotY);
                squares.ForEach(delegate (Squares s)
                {
                    if (Robot.transform.position == s.pos)
                    {

                        if (s.colorName == Red)
                        {
                            wrong = true;
                        }
                    }
                });

            }
        }

        if(Robot.transform.position == GreenPos)
        {
            done = true;
        }
        
    }
    public void reset()
    {
            squares.ForEach(delegate (Squares s)
            {
                if (s.colorName == Blue)
                {
                    Robot.transform.position = s.pos;
                    for (int i = 0; i < 20; i++)
                    {
                        if (xPos[i] == Robot.transform.position.x)
                        {
                            robotX = i;

                        }
                        if (yPos[i] == Robot.transform.position.y)
                        {
                            robotY = i;

                        }
                    }
                    currentState = findState(robotX, robotY);
                }
            });
        statePath.Clear();
        rewardAtStatePath = 0;
        stepAtStatePath = 0;




    }

    IEnumerator move()
    {
        for (int i = 0; i < 20; i++)
        {
            if (xPos[i] == Robot.transform.position.x)
            {
                robotX = i;

            }
            if (yPos[i] == Robot.transform.position.y)
            {
                robotY = i;

            }
        }
        
       while(true)
        {
            if (wrong)
            {
                reset();
                wrong = false;
                
            }
            currentState = findState(robotX, robotY);
            statePath.Add(Robot.transform.position);
            int action = chooseAction(currentState);

            robotMove(action, robotX, robotY);

            yield return new WaitForSeconds(0.01f);

            old_value = Qmatris[currentState, action];
            next_action = chooseAction(nextState);
            next_max = Qmatris[nextState, next_action];

            next_value = rewardAtStep + discountFactor * next_max;

            Qmatris[currentState, action] = next_value;
            currentState = nextState;
            if (done)
            {
                
                loop -= 1;
                
                if(bestStatePath.Count == 0)
                {
                    for(int i = 0; i < statePath.Count; i++)
                    {
                        bestStatePath.Add(statePath[i]);
                    }
                    
                }
                else
                {
                    if (bestStatePath.Count > statePath.Count)
                    {
                        bestStatePath.Clear();
                        for (int i = 0; i < statePath.Count; i++)
                        {
                            bestStatePath.Add(statePath[i]);
                        }
                        
                    }
                }
                if (loop == 0)
                {
                    for (int i = 0; i < bestStatePath.Count; i++)
                    {
                        
                        squares.ForEach(delegate (Squares s)
                        {
                            if (bestStatePath[i] == s.pos)
                            {
                                s.colorName = Blue;
                                s.obj.gameObject.GetComponent<SpriteRenderer>().material = Blue;
                            }
                        });


                    }
                    song.Stop();
                    break;
                }
                else
                {
                    
                    reset();
                    done = false;
                }


            }
            
            

        }
    }

}
