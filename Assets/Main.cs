using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

    private int mapsize = 50;
    private Block[,] blocks;
    private Block[,] objects;
    private GameObject player;
    private List<Ball> balls;

    private float movespeed = 2f;
    private float vaccel = -16f;
    private float maxDropSpeed = 4f;
    private float jumpSpeed = 5f;
    private float bigJumpTime = 0.2f;
    private float turnTime = 0.1f;
    private float hitboxMargin = 0.10f;
    private float ballspeed = 3f;
    private float ballRadius = 0.15f;

    private List<Block> playerBlocks;
    private float playerVSpeed;
    private int playerDir = 1;
    private float fallHeight = 0;

    private KeyCode jumpButton = KeyCode.Space;


    private float jumptimer = 0;
    private float turntimer = 0;

    // Use this for initialization
    void Start () {
        blocks = new Block[mapsize, mapsize];
        objects = new Block[mapsize, mapsize];
        player = GameObject.FindGameObjectWithTag("Player");
        playerVSpeed = 0;
        fallHeight = 0;


        var world = GameObject.FindGameObjectWithTag("World");
        foreach (Transform child in world.transform)
        {
            var x = Mathf.FloorToInt(child.transform.position.x);
            var y = Mathf.FloorToInt(child.transform.position.y);
            

            if (child.CompareTag("End"))
            {
                objects[x, y] = new Block(child.gameObject, x, y);
            } else
            {
                blocks[x, y] = new Block(child.gameObject, x, y);
            }
        }

        playerBlocks = new List<Block>();
        balls = new List<Ball>();

    }
	
	// Update is called once per frame
	void Update () {

        updatePlayer();

        updateBlocks();

        updateBalls();
    }

    private void updatePlayer()
    {
        jumptimer -= Time.deltaTime;
        turntimer -= Time.deltaTime;

        if (playerGroundDist() == 0)
        {
            if (fallHeight - player.transform.position.y > 3)
            {
                Debug.Log("death");
                SceneManager.LoadScene("scene");
            }
            fallHeight = 0;
        } else
        {
            fallHeight = Mathf.Max(fallHeight, player.transform.position.y);
        }

        if (Input.GetKey(jumpButton) && playerGroundDist() == 0)
        {
            playerVSpeed = jumpSpeed;
            jumptimer = bigJumpTime;
        }

        if (Input.GetKey(jumpButton) && jumptimer > 0)
        {
            playerVSpeed = jumpSpeed;
        }

        if (playerCeilingDist() == 0)
        {
            playerVSpeed = 0;
        }

        playerVSpeed += vaccel * Time.deltaTime;

        if (playerVSpeed < -maxDropSpeed)
        {
            playerVSpeed = -maxDropSpeed;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (playerDir < 0)
            {
                playerDir = 1;
                turntimer = turnTime;
            } else if (turntimer < 0)
            {
                player.transform.position = new Vector3(player.transform.position.x + movespeed * Time.deltaTime, player.transform.position.y, player.transform.position.z);
                var x = Mathf.CeilToInt(player.transform.position.x);
                var y1 = Mathf.FloorToInt(player.transform.position.y + hitboxMargin);
                var y2 = Mathf.CeilToInt(player.transform.position.y - hitboxMargin);
                if (blocks[x, y1] != null || blocks[x, y2] != null || blocks[x, y2 + 1] != null)
                {
                    player.transform.position = new Vector3(x - 1, player.transform.position.y, player.transform.position.z);
                }
            }

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (playerDir > 0)
            {
                playerDir = -1;
                turntimer = turnTime;
            }
            else if (turntimer < 0)
            {
                playerDir = -1;
                player.transform.position = new Vector3(player.transform.position.x - movespeed * Time.deltaTime, player.transform.position.y, player.transform.position.z);

                var x = Mathf.FloorToInt(player.transform.position.x);
                var y1 = Mathf.FloorToInt(player.transform.position.y + hitboxMargin);
                var y2 = Mathf.CeilToInt(player.transform.position.y - hitboxMargin);
                if (blocks[x, y1] != null || blocks[x, y2] != null || blocks[x, y2 + 1] != null)
                {
                    player.transform.position = new Vector3(x + 1, player.transform.position.y, player.transform.position.z);
                }
            }
        }


        if (playerVSpeed > 0)
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + playerVSpeed * Time.deltaTime, player.transform.position.z);

            var y = Mathf.CeilToInt(player.transform.position.y);
            var x1 = Mathf.FloorToInt(player.transform.position.x + hitboxMargin);
            var x2 = Mathf.CeilToInt(player.transform.position.x - hitboxMargin);
            if (blocks[x1, y + 1] != null || blocks[x2, y + 1] != null)
            {
                player.transform.position = new Vector3(player.transform.position.x, y - 1, player.transform.position.z);
            }
        }

        if (playerVSpeed < 0)
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + playerVSpeed * Time.deltaTime, player.transform.position.z);

            var y = Mathf.FloorToInt(player.transform.position.y);
            var x1 = Mathf.FloorToInt(player.transform.position.x + hitboxMargin);
            var x2 = Mathf.CeilToInt(player.transform.position.x - hitboxMargin);
            if (blocks[x1, y] != null || blocks[x2, y] != null)
            {
                player.transform.position = new Vector3(player.transform.position.x, y + 1, player.transform.position.z);
            }
        }

        //action tir de block
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                var x = Mathf.RoundToInt(player.transform.position.x);
                var y = Mathf.FloorToInt(player.transform.position.y);
                spawnPlayerBlock(x, y - 1);
            } else
            {
                var x = Mathf.FloorToInt(player.transform.position.x);
                if (playerDir > 0)
                {
                    x = Mathf.CeilToInt(player.transform.position.x);
                }
                var y = player.transform.position.y;
                spawnPlayerBlock(x + playerDir, y + 1);
            }
        }

        //action tir de boule
        if (Input.GetKeyDown(KeyCode.R))
        {
            var x = Mathf.FloorToInt(player.transform.position.x);
            if (playerDir > 0)
            {
                x = Mathf.CeilToInt(player.transform.position.x);
            }
            var y = player.transform.position.y;

            if (Input.GetKey(KeyCode.DownArrow))
            {
                y = y - 1;
            }
            shootBall(x, y + 1, playerDir);
        }

        //check collectibles
        var posx = Mathf.RoundToInt(player.transform.position.x);
        var posy = Mathf.RoundToInt(player.transform.position.y);
        if (objects[posx, posy] != null && objects[posx, posy].Obj.CompareTag("End"))
        {
            Debug.Log("end");
            SceneManager.LoadScene("scene");
        }

        //mouvement camera
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }

    private void updateBlocks()
    {
        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (blocks[i, j] != null && !blocks[i, j].IsFixed)
                {
                    Block block = blocks[i, j];
                    block.Vspeed += vaccel * Time.deltaTime;

                    if (block.Vspeed < -maxDropSpeed)
                    {
                        block.Vspeed = -maxDropSpeed;
                    }

                    if (block.Vspeed < 0)
                    {
                        block.Obj.transform.position = new Vector3(block.Obj.transform.position.x, block.Obj.transform.position.y + block.Vspeed * Time.deltaTime, block.Obj.transform.position.z);

                        var y = Mathf.FloorToInt(block.Obj.transform.position.y);
                        var x = Mathf.RoundToInt(block.Obj.transform.position.x);
                        
                        if (y < block.Y)
                        {
                            if (blocks[x, y] != null)
                            {
                                block.Obj.transform.position = new Vector3(block.Obj.transform.position.x, y + 1, block.Obj.transform.position.z);
                            } else
                            {
                                blocks[block.X, block.Y] = null;
                                blocks[x, y] = block;
                                block.X = x;
                                block.Y = y;
                            }
                        }
                    }
                }
            }
        }
    }

    private void updateBalls()
    {
        List<Ball> toDestroy = new List<Ball>();
        foreach (Ball ball in balls)
        {
            var oldpos = ball.Obj.transform.position;
            ball.Obj.transform.position = new Vector3(oldpos.x + ball.Speed * Time.deltaTime, oldpos.y, oldpos.z);

            var posx1 = Mathf.RoundToInt(ball.Obj.transform.position.x + ballRadius);
            var posy1 = Mathf.RoundToInt(ball.Obj.transform.position.y + ballRadius);

            if (checkBallCollision(ball, posx1, posy1))
            {
                toDestroy.Add(ball);
            }

            var posx2 = Mathf.RoundToInt(ball.Obj.transform.position.x + ballRadius);
            var posy2 = Mathf.RoundToInt(ball.Obj.transform.position.y - ballRadius);

            if (checkBallCollision(ball, posx2, posy2))
            {
                toDestroy.Add(ball);
            }

            var posx3 = Mathf.RoundToInt(ball.Obj.transform.position.x - ballRadius);
            var posy3 = Mathf.RoundToInt(ball.Obj.transform.position.y - ballRadius);

            if (checkBallCollision(ball, posx3, posy3))
            {
                toDestroy.Add(ball);
            }

            var posx4 = Mathf.RoundToInt(ball.Obj.transform.position.x - ballRadius);
            var posy4 = Mathf.RoundToInt(ball.Obj.transform.position.y + ballRadius);

            if (checkBallCollision(ball, posx4, posy4))
            {
                toDestroy.Add(ball);
            }
        }

        foreach (Ball ball in toDestroy)
        {
            balls.Remove(ball);
            Destroy(ball.Obj);
        }
    }

    private float playerGroundDist()
    {
        var y = Mathf.FloorToInt(player.transform.position.y);
        var dist = player.transform.position.y - y;

        var x1 = Mathf.FloorToInt(player.transform.position.x);
        var x2 = Mathf.CeilToInt(player.transform.position.x);

        int i = 1;
        while (y - i > 0 && blocks[x1, y - i] == null && blocks[x2, y - i] == null)
        {
            dist = dist + 1;
            i++;
        }

        return dist;
    }

    private float playerCeilingDist()
    {
        var y = Mathf.CeilToInt(player.transform.position.y);
        var dist = y - player.transform.position.y;

        var x1 = Mathf.FloorToInt(player.transform.position.x);
        var x2 = Mathf.CeilToInt(player.transform.position.x);

        int i = 1;
        while (y + i < mapsize && blocks[x1, y + i] == null && blocks[x2, y + i] == null)
        {
            dist = dist + 1;
            i++;
        }

        return dist - 1;
    }

    private Block spawnBlock(float x, float y)
    {
        int x2 = Mathf.RoundToInt(x);
        int y2 = Mathf.FloorToInt(y);

        if (blocks[x2, y2] == null)
        {
            GameObject obj = (GameObject)Instantiate(Resources.Load("PlayerWall"), new Vector3(x2, y, 0), Quaternion.identity);
            Block b = new Block(obj, x2, y2);
            b.IsFixed = false;
            blocks[x2, y2] = b;
            return b;
        }

        return null;
    }

    private void spawnPlayerBlock(float x, float y)
    {
        Block block = spawnBlock(x, y);

        if (block != null)
        {
            block.IsDestrutible = true;
            playerBlocks.Add(block);
            if (playerBlocks.Count > 3)
            {
                Block removed = playerBlocks[0];
                playerBlocks.RemoveAt(0);
                removeBlock(removed);
            }
        }
    }

    private void shootBall(float x, float y, float dir)
    {
        GameObject obj = (GameObject)Instantiate(Resources.Load("Ball"), new Vector3(x, y, 0), Quaternion.identity);
        Ball ball = new Ball(obj, ballspeed * dir);
        balls.Add(ball);
    }

    private bool checkBallCollision(Ball ball, int x, int y)
    {
        bool res = false;
        if (x < 0 || x >= mapsize || y < 0 || y >= mapsize)
        {
            res = true;

        }
        else if (blocks[x, y] != null)
        {
            res = true;
            removeBlock(blocks[x, y]);
        }

        return res;
    }

    private void removeBlock(Block block)
    {
        Destroy(block.Obj);
        blocks[block.X, block.Y] = null;
        playerBlocks.Remove(block);
    }


}
