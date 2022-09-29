using System.Collections.Generic;
using UnityEngine;

public class PlayerBoard : MonoBehaviour
{
    [Header("Player board design")]
    [SerializeField] private GameObject tileObject;
    [SerializeField] private float padding = 11.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    // constannts - board size
    private const int tileCount_X = 10;
    private const int tileConut_Y = 10;

    // grid
    private GameObject[,] tiles;
    public List<Vector2Int> placementTiles = new List<Vector2Int>();

    [Header("Ships")]
    [SerializeField] private GameObject[] shipPrefabs;
    private List<Ship> ship;
    // placeholder for tiles that my be used
    private List<Vector2Int> usedTiles = new List<Vector2Int>();
    private Vector3 rotation = Vector3.zero;

    [Header("Player")]
    public bool player; // true = player one, false = player two

    private void Awake()
    {
        GenerateBoard(tileCount_X, tileConut_Y);
    }

    private void Start()
    {
        player = true;
        SpawnShips(player);
    }

    // method for generation tiles on a grid which represent a game board
    private void GenerateBoard(int tileCountX, int tileCountY)
    {
        //boardCenter = transform.position;

        tiles = new GameObject[tileCountX, tileCountY];

        for (int i = 0; i < tileCountX; i++)
        {
            for (int j = 0; j < tileCountY; j++)
            {
                Vector3 pos = new Vector3(j * padding, 0, i * padding);
                tiles[i,j] = Instantiate(tileObject,  pos, Quaternion.identity);
                tiles[i,j].transform.parent = gameObject.transform;
                placementTiles.Add(new Vector2Int(i,j));
            }
        }
    }

    public void SpawnShips(bool player)
    {
        ship = new List<Ship>();

        ship.Add(SpawnSingleShip(ShipType.Battleship, player));
        ship.Add(SpawnSingleShip(ShipType.Cruiser, player));
        ship.Add(SpawnSingleShip(ShipType.Destroyer, player));
        ship.Add(SpawnSingleShip(ShipType.Frigate, player));

        // because there has to be two 1x1 ships
        for (int i = 0; i < 2; i++)
        {
            ship.Add(SpawnSingleShip(ShipType.Corvette, player));
        }

    }

    private Ship SpawnSingleShip(ShipType type, bool player)
    { 
        Ship shipPiece = Instantiate(shipPrefabs[(int)type - 1], transform).GetComponent<Ship>();

        shipPiece.ShipType = type;
        shipPiece.player = player;

        return shipPiece;
    }

   // method for valid ship placement
    public void PositionShips()
    {

        for (int i = 0; i < ship.Count; i++)
        {
            bool tileSearch = true;
            Debug.Log(ship[i].Name);
            while (tileSearch)
            {
                usedTiles.Clear();

                int startRow = Random.Range(0, 9);
                int startColumn = Random.Range(0, 9);
                int endRow = startRow;
                int endColumn = startColumn;
                int orientation = Random.Range(0, 3); // rotation of ship : 0 = 0; 1 = 90, 2 = 180, 3 = 270 

                Debug.Log(startRow + " " + startColumn + " " + orientation);

                int removeTile = 0;

                // foreach orientation get tiles that will be used
                // 0
                if (orientation == 0)
                {
                    for (int w = 1; w <= ship[i].Width; w++)
                    {
                        usedTiles.Add(new Vector2Int(endRow, startColumn));
                        endRow--;
                    }

                    rotation = new Vector3(0, 0, 0);
                }
                //90
                else if (orientation == 1)
                {
                    for (int w = 1; w <= ship[i].Width; w++)
                    {
                        usedTiles.Add(new Vector2Int(startRow, endColumn));
                        endColumn--;

                    }
                    rotation = new Vector3(0, 90, 0);
                }
                //180
                else if (orientation == 2)
                {
                    for (int w = 1; w <= ship[i].Width; w++)
                    {
                        usedTiles.Add(new Vector2Int(endRow, startColumn));
                        endRow++;

                    }
                    rotation = new Vector3(0, 180, 0);
                }
                //270
                else if (orientation == 3)
                {
                    for (int w = 1; w <= ship[i].Width; w++)
                    {
                        usedTiles.Add(new Vector2Int(startRow, endColumn));
                        endColumn++;

                    }
                    rotation = new Vector3(0, 270, 0);
                }

                // cannot place ships beyond the boundaries of the board
                if (endRow > 9 || endColumn > 9 || endColumn < 0 || endRow < 0)
                {
                    tileSearch = true;
                    continue;
                }

                // check if specified tiles are occupied
                for (int x = 0; x < usedTiles.Count; x++)
                {
                    for (int t = 0; t < placementTiles.Count; t++)
                    {
                        if (placementTiles[t].x == usedTiles[x].x && placementTiles[t].y == usedTiles[x].y)
                        {
                            removeTile++;
                        }
                    }
                }
                // if no tiles are occupied then remove them from placement tile list -- this represent occupation of tiles
                if (removeTile == ship[i].Width)
                {
                    for (int x = 0; x < usedTiles.Count; x++)
                    {
                        for (int t = 0; t < placementTiles.Count; t++)
                        {
                            if (placementTiles[t].x == usedTiles[x].x && placementTiles[t].y == usedTiles[x].y)
                            {

                                tiles[placementTiles[t].x, placementTiles[t].y].gameObject.SetActive(false);
                                placementTiles.RemoveAt(t);
                            }
                        }
                    }
                }
                tileSearch = false;
            }

            // check which rotation does ship have for rightful placement
            if (rotation.y == 0)
            {
                Vector3 tilePosition = GetPositionCenterForVerticalRotation(usedTiles[0], usedTiles[ship[i].Width - 1]);
                ship[i].transform.Rotate(rotation);
                ship[i].transform.position = tilePosition;
            }
            else if (rotation.y == 90)
            {
                Vector3 tilePosition = GetPositionCenterForHorizontalRotation(usedTiles[0], usedTiles[ship[i].Width - 1]);
                ship[i].transform.Rotate(rotation);
                ship[i].transform.position = tilePosition;
            }
            else if (rotation.y == 180)
            {
                Vector3 tilePosition = GetPositionCenterForVerticalRotation(usedTiles[0], usedTiles[ship[i].Width - 1]);
                ship[i].transform.Rotate(rotation);
                ship[i].transform.position = tilePosition;
            }
            else if (rotation.y == 270)
            {
                Vector3 tilePosition = GetPositionCenterForHorizontalRotation(usedTiles[0], usedTiles[ship[i].Width - 1]);
                ship[i].transform.Rotate(rotation);
                ship[i].transform.position = tilePosition;
            }

        }
    }

    // method for getting centar when ship has vertical orientation -- column center stays the same, but row center has to be calculated 
    private Vector3 GetPositionCenterForVerticalRotation(Vector2Int firstTile, Vector2Int lastTile)
    {
        return new Vector3(tiles[firstTile.x, firstTile.y].transform.position.x, 0, (tiles[firstTile.x, firstTile.y].transform.position.z + tiles[lastTile.x, lastTile.y].transform.position.z) / 2);
    }

    // method for getting centar when ship has horizontal orientation -- row center stays the same, but column center has to be calculated
    private Vector3 GetPositionCenterForHorizontalRotation(Vector2Int firstTile, Vector2Int lastTile)
    {
        return new Vector3((tiles[firstTile.x, firstTile.y].transform.position.x + tiles[lastTile.x, lastTile.y].transform.position.x) / 2, 0, tiles[firstTile.x, firstTile.y].transform.position.z);
    }



}