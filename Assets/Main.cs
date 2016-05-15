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
    private List<Enemy> enemys;

    private AudioSource source;
    public AudioClip deathclip;

    private float movespeed = 2.5f;
    private float vaccel = -18f;
    private float maxDropSpeed = 4.5f;
    private float jumpSpeed = 5f;
    private float bigJumpTime = 0.2f;
    private float turnTime = 0.1f;
    private float hitboxMargin = 0.12f;
    private float ballspeed = 5f;
    private float ballRadius = 0.15f;
    private float fallDeathHeight = 5f;
    private float jumpcooldowntime = 0.1f;
    private float shootcooldowntime = 0.3f;
    private float spawnBlockTime = 0.0f;

    private List<Block> playerBlocks;
    private float playerVSpeed;
    private int playerDir = 1;
    private float fallHeight = 0;
    private bool loadingLevel = false;


    private float jumptimer = 0;
    private float jumpcooldowntimer = 0;
    private float turntimer = 0;
    private float shootcooldowntimer = 0;
    private float freezeMoveTimer = 0;
    private float loadLevelTimer = 0;

    // Use this for initialization
    void Start () {
        blocks = new Block[mapsize, mapsize];
        objects = new Block[mapsize, mapsize];
        enemys = new List<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
        source = GetComponent<AudioSource>();
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
                if (child.CompareTag("Destructible"))
                {
                    blocks[x, y].IsDestrutible = true;
                    blocks[x, y].IsFixed = false;
                }
                if (child.CompareTag("FixedDestructible"))
                {
                    blocks[x, y].IsDestrutible = true;
                    blocks[x, y].IsFixed = true;
                }
                if (child.CompareTag("Indestructible"))
                {
                    blocks[x, y].IsDestrutible = false;
                    blocks[x, y].IsFixed = false;
                }
                if (child.CompareTag("Kill"))
                {
                    blocks[x, y].IsDestrutible = false;
                    blocks[x, y].IsFixed = true;
                    blocks[x, y].IsKill = true;
                }
                if (child.CompareTag("Enemy"))
                {
                    var prm = child.GetComponent<Params>();
                    Enemy enemy = new Enemy(child.gameObject, x, y, prm.deltaMove, prm.speed, prm.vertical);
                    enemys.Add(enemy);
                }
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

        updateEnemys();
    }

    private void updatePlayer()
    {
        var animation = player.transform.GetChild(0).GetChild(0).GetComponent<Animation>();
        jumptimer -= Time.deltaTime;
        turntimer -= Time.deltaTime;
        jumpcooldowntimer -= Time.deltaTime;
        shootcooldowntimer -= Time.deltaTime;
        freezeMoveTimer -= Time.deltaTime;
        loadLevelTimer -= Time.deltaTime;


        var walk = false;
        var jump = false;
        var fall = false;
        var shoot1 = false;
        var shoot2 = false;

        if (playerGroundDist() == 0)
        {
            //check fall
            if (fallHeight - player.transform.position.y > fallDeathHeight)
            {
                Debug.Log("fall death");
                killPlayer();
            }
            if (fallHeight > 0)
            {
                fall = true;
                jumpcooldowntimer = jumpcooldowntime;
            }
            fallHeight = 0;

            //check lava
            var x = Mathf.RoundToInt(player.transform.position.x);
            var y = Mathf.RoundToInt(player.transform.position.y);
            if (blocks[x, y - 1] != null && blocks[x, y - 1].IsKill)
            {
                killPlayer();
            }
        } else
        {
            jump = true;
            fallHeight = Mathf.Max(fallHeight, player.transform.position.y);
        }

        if (Input.GetButton("Jump") && playerGroundDist() == 0 && playerCeilingDist() > 0 && jumpcooldowntimer < 0)
        {
            playerVSpeed = jumpSpeed;
            jumptimer = bigJumpTime;
        }

        if (Input.GetButton("Jump") && jumptimer > 0)
        {
            playerVSpeed = jumpSpeed;
        }

        if (playerCeilingDist() == 0)
        {
            playerVSpeed = 0;

            //check lava
            var x = Mathf.RoundToInt(player.transform.position.x);
            var y = Mathf.RoundToInt(player.transform.position.y);
            if (blocks[x, y + 2] != null && blocks[x, y + 2].IsKill)
            {
                Debug.Log("lava death");
                killPlayer();
            }
        }

        if (playerCeilingDist() < - 0.5f)
        {
            Debug.Log("head death");
            //killPlayer();
        }


        if (freezeMoveTimer < 0)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                walk = true;
                if (playerDir < 0)
                {
                    playerDir = 1;
                    turntimer = turnTime;
                    player.transform.Rotate(0, 180, 0);
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

            if (Input.GetAxis("Horizontal") < 0)
            {
                walk = true;
                if (playerDir > 0)
                {
                    playerDir = -1;
                    turntimer = turnTime;
                    player.transform.Rotate(0, 180, 0);
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

            playerVSpeed += vaccel * Time.deltaTime;

            if (playerVSpeed < -maxDropSpeed)
            {
                playerVSpeed = -maxDropSpeed;
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
        }
        
        //action tir de block
        if (Input.GetAxis("Fire1") > 0 && shootcooldowntimer < 0)
        {
            shoot1 = true;
            shootcooldowntimer = shootcooldowntime;
            if (playerGroundDist() > 0 && Input.GetAxis("Vertical") < 0)
            {
                var x = Mathf.RoundToInt(player.transform.position.x);
                var y = player.transform.position.y;
                bool okSpawn = spawnPlayerBlock(x, y - 1);
                if (spawnBlockTime > 0 && okSpawn)
                {
                    playerVSpeed = 0;
                    freezeMoveTimer = spawnBlockTime;
                }
            }
            else if (Input.GetAxis("Vertical") > 0)
            {
                /*var x = Mathf.RoundToInt(player.transform.position.x);
                var y = player.transform.position.y + 1;
                spawnPlayerBlock(x, y + 1.5f);*/
            } else
            {
                var x = Mathf.FloorToInt(player.transform.position.x);
                if (playerDir > 0)
                {
                    x = Mathf.CeilToInt(player.transform.position.x);
                }
                var y = player.transform.position.y;

                if (Input.GetAxis("Vertical") < 0)
                {
                    y = y - 1;
                }
                spawnPlayerBlock(x + playerDir, y + 1);
            }
        }

        //action tir de boule
        if (Input.GetAxis("Fire2") > 0 && shootcooldowntimer < 0)
        {
            shoot2 = true;
            shootcooldowntimer = shootcooldowntime;
            if (playerGroundDist() > 0 && Input.GetAxis("Vertical") < 0)
            {
                var x = player.transform.position.x;
                var y = player.transform.position.y;
                shootBall(x, y, -1, true);
            }
            else if (Input.GetAxis("Vertical") > 0)
            {
                var x = player.transform.position.x;
                var y = player.transform.position.y + 1;
                shootBall(x, y, 1, true);
            }
            else
            {
                var x = Mathf.FloorToInt(player.transform.position.x);
                if (playerDir > 0)
                {
                    x = Mathf.CeilToInt(player.transform.position.x);
                }
                var y = player.transform.position.y;

                if (Input.GetAxis("Vertical") < 0)
                {
                    y = y - 1;
                }
                shootBall(x, y + 1, playerDir, false);
            }
        }


        //animation
        if (shoot1)
        {
            //animation.Play("jump"); //todo remplacer jump
            //animation["jump"].speed = 10f;
        } else if (shoot2)
        {
            //animation.Play("jump"); //todo remplacer jump
            //animation["jump"].speed = 10f;
        }
        else if (jump)
        {
            animation.Play("jump");
            animation["jump"].speed = 1f;
        }
        else if (fall)
        {
            animation.Play("fall");
            animation["fall"].speed = 3f;
        }
        else if (walk)
        {
            if (!animation.IsPlaying("fall") && !animation.IsPlaying("jump"))
            {
                animation.Play("walk");
            }
            animation["walk"].speed = 1.5f;
        }
        else
        {
            if (!animation.IsPlaying("fall") && !animation.IsPlaying("jump"))
            {
                animation.Play("idle");
            }
        }

        //check collectibles
        var posx = Mathf.RoundToInt(player.transform.position.x);
        var posy = Mathf.RoundToInt(player.transform.position.y);
        if (objects[posx, posy] != null && objects[posx, posy].Obj.CompareTag("End"))
        {
            Debug.Log("end");
            reloadLevel(1f);
        }

        //mouvement camera
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);

        //retry
        if (Input.GetButtonDown("Cancel"))
        {
            reloadLevel(1f);
        }

        if (loadingLevel && loadLevelTimer < 0)
        {
            int scene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
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
                    block.spawnTimer -= Time.deltaTime;

                    if (block.spawnTimer < 0)
                    {
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
                                }
                                else
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
    }

    private void updateBalls()
    {
        List<Ball> toDestroy = new List<Ball>();
        foreach (Ball ball in balls)
        {
            var oldpos = ball.Obj.transform.position;
            if (!ball.Vertical)
            {
                ball.Obj.transform.position = new Vector3(oldpos.x + ball.Speed * Time.deltaTime, oldpos.y, oldpos.z);
            } else
            {
                ball.Obj.transform.position = new Vector3(oldpos.x, oldpos.y + ball.Speed * Time.deltaTime, oldpos.z);
            }

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

    private void updateEnemys()
    {
        foreach (Enemy enemy in enemys)
        {
            
            var origx = enemy.OrigX;
            var origy = enemy.OrigY;
            var targetx = enemy.OrigX;
            var targety = enemy.OrigY;

            if (enemy.IsVertical)
            {
                if (enemy.CurrentDir > 0)
                {
                    targety += enemy.DeltaMove;
                }

                var newy = enemy.Obj.transform.position.y + enemy.Speed * Time.deltaTime * Mathf.Sign(enemy.DeltaMove) * enemy.CurrentDir;
                if (enemy.DeltaMove * enemy.CurrentDir > 0 && newy > targety)
                {
                    newy = targety;
                    enemy.CurrentDir = -enemy.CurrentDir;
                }
                if (enemy.DeltaMove * enemy.CurrentDir < 0 && newy < targety)
                {
                    newy = targety;
                    enemy.CurrentDir = -enemy.CurrentDir;
                }

                if (blocks[origx, Mathf.RoundToInt(newy + Mathf.Sign(enemy.DeltaMove * enemy.CurrentDir))] != null)
                {
                    //newy = Mathf.RoundToInt(newy);
                    enemy.CurrentDir = -enemy.CurrentDir;
                }


                enemy.Obj.transform.position = new Vector3(enemy.Obj.transform.position.x, newy, enemy.Obj.transform.position.z);
            } else
            {
                if (enemy.CurrentDir > 0)
                {
                    targetx += enemy.DeltaMove;
                }

                var newx = enemy.Obj.transform.position.x + enemy.Speed * Time.deltaTime * Mathf.Sign(enemy.DeltaMove) * enemy.CurrentDir;
                if (enemy.DeltaMove * enemy.CurrentDir > 0 && newx > targetx)
                {
                    newx = targetx;
                    enemy.CurrentDir = -enemy.CurrentDir;
                }
                if (enemy.DeltaMove * enemy.CurrentDir < 0 && newx < targetx)
                {
                    newx = targetx;
                    enemy.CurrentDir = -enemy.CurrentDir;
                }

                if (blocks[origy, Mathf.RoundToInt(newx + Mathf.Sign(enemy.DeltaMove * enemy.CurrentDir))] != null)
                {
                    //newx = Mathf.RoundToInt(newx);
                    enemy.CurrentDir = -enemy.CurrentDir;
                }

                enemy.Obj.transform.position = new Vector3(newx, enemy.Obj.transform.position.y, enemy.Obj.transform.position.z);
            }
        }
    }

    private float playerGroundDist()
    {
        var y = Mathf.FloorToInt(player.transform.position.y);
        var dist = player.transform.position.y - y;

        var x1 = Mathf.FloorToInt(player.transform.position.x + hitboxMargin);
        var x2 = Mathf.CeilToInt(player.transform.position.x - hitboxMargin);

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

        var x1 = Mathf.FloorToInt(player.transform.position.x + hitboxMargin);
        var x2 = Mathf.CeilToInt(player.transform.position.x - hitboxMargin);

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
        int y3 = Mathf.CeilToInt(y);

        if (blocks[x2, y2] == null && blocks[x2, y3] == null)
        {
            GameObject obj = (GameObject)Instantiate(Resources.Load("prefabs/caisse_orange"), new Vector3(x2, y, 0), Quaternion.identity);
            Block b = new Block(obj, x2, y2);
            b.IsFixed = false;
            blocks[x2, y2] = b;
            return b;
        }

        return null;
    }

    private bool spawnPlayerBlock(float x, float y)
    {
        Block block = spawnBlock(x, y);

        if (block != null)
        {
            block.spawnTimer = spawnBlockTime;
            block.IsDestrutible = true;
            playerBlocks.Add(block);
            if (playerBlocks.Count > 3)
            {
                Block removed = playerBlocks[0];
                playerBlocks.RemoveAt(0);
                removeBlock(removed);
            }
            return true;
        }
        return false;
    }

    private void shootBall(float x, float y, float dir, bool vert)
    {
        GameObject obj = (GameObject)Instantiate(Resources.Load("Ball"), new Vector3(x, y, 0), Quaternion.identity);
        Ball ball = new Ball(obj, ballspeed * dir);
        ball.Vertical = vert;
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
            if (blocks[x, y].IsDestrutible)
            {
                removeBlock(blocks[x, y]);
            }
        }

        return res;
    }

    private void removeBlock(Block block)
    {
        Destroy(block.Obj);
        blocks[block.X, block.Y] = null;
        playerBlocks.Remove(block);
    }

    private void reloadLevel(float delay)
    {
        loadingLevel = true;
        loadLevelTimer = delay;
    }

    private void killPlayer()
    {
        Debug.Log("death");
        source.PlayOneShot(deathclip);
        reloadLevel(1f);
    }


}
