using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathing;

public class SceneManager : MonoBehaviour
{

    public List<Vector3> initialNodeCoordinate; // save the initial coordinate of each row.

    public GameObject hexNode;
    public GameObject objectManager;
    public List<Material> nodeMaterials;

    private List<GameObject> allNodes = new List<GameObject>(); // all nodes will be added in this list.

    private int mapSize = 7;
    private Vector3 hexNodeScale = new Vector3(5, 5, 5);

    private List<MyNode> globalNodes;

    private (MyNode, MyNode) path;


    // Start is called before the first frame update
    void Start()
    {

        globalNodes = new List<MyNode>();

		/// 0 , 1 , 2 , 3 , 4 , 5 , 6
		///  7 , 8 , 9 , 10, 11, 12, 13
		/// 14, 15, 16, 17, 18, 19, 20
		/// ..........................
		///  49, 50, 51, 52, 53, 54, 55
		///  
        /// 
        /// 
        /// 
		/// (0, 0)  (1, 0)  (2, 0)  (3, 0)  ...  (6, 0)
		///   (0, 1)  (1, 1)  (2, 1)  (3, 1)  ...  (6, 1)
		/// (0, 2)  (1, 2)  (2, 2)  (3, 2)  ...  (6, 2)
		/// ..........................
		///   (0, 6)  (1, 6)  (2, 6)  (3, 6)  ...  (6, 6)

        int x = 0;
        int y = 0;


        foreach (Vector3 p in initialNodeCoordinate)
        {
            for(int i = 0; i < mapSize; i++)
            {

                float positionX = p.x + 5.0f * i;
                Vector3 initialPosition = new Vector3(positionX, p.y, p.z);
                GameObject temObject = GameObject.Instantiate(hexNode, initialPosition, new Quaternion(), objectManager.transform);
                temObject.name = (i + y * mapSize).ToString();
                temObject.transform.localScale = hexNodeScale;

                int nodeType = Random.Range(0, 5);

                // 0 desert 1 forest 2 grass 3 mountain 4 water
                // 5 days   3 days   1 day   10 days    positive infinity
                string[] typenames = { "desert", "forest", "grass", "mountain", "water" };

                temObject.GetComponent<Renderer>().material = new Material(nodeMaterials[nodeType]);


                MyNode temNode = new MyNode(typenames[nodeType], nodeType, x, y, i + y * mapSize, null);
                globalNodes.Add(temNode);

                allNodes.Add(temObject);

                x += 1;
            }
            x = 0;
            y += 1;
        }

        InitializeNeighbours(globalNodes);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            foreach (GameObject i in allNodes)
            {
                i.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));
            }
            path.Item1 = path.Item2 = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            int index = System.Convert.ToInt32(hit.transform.name);
            if (path.Item1 == null)
            {
                foreach (GameObject i in allNodes)
                {
                    i.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));
                }
                hit.collider.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 1, 0));
            }
            else if (path.Item1 != null && path.Item2 == null)
            {
                MyNode currentPointingNode = globalNodes[index];
                foreach (GameObject i in allNodes)
                {
                    i.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));
                }
                IList<IAStarNode> result = AStar.GetPath(path.Item1, currentPointingNode);
                foreach (IAStarNode i in result)
                {
                    MyNode myNode = (MyNode)i;
                    // Debug.Log(myNode.typeName + ", " + myNode.x + ", " + myNode.y + ", " + myNode.globalIndex);
                    allNodes[myNode.globalIndex].gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 1, 0));

                }
            }
            else
            {
                IList<IAStarNode> result = AStar.GetPath(path.Item1, path.Item2);
                MyNode startNode = (MyNode)result[0];
                MyNode endNode = (MyNode)result[result.Count - 1];

                Debug.Log("You start from (" + startNode.x + ", " + startNode.y + "), end at (" + endNode.x + ", " + endNode.y + ")");
                Debug.Log("Total Length: " + (result.Count - 1));

                // float cost = 0;
                int[] typeList = { 0, 0, 0, 0, 0};
                foreach (IAStarNode i in result)
                {
                    MyNode myNode = (MyNode)i;
                    // cost += myNode.typeCost[myNode.typeIndex];
                    typeList[myNode.typeIndex] += 1;
                }

                Debug.Log("You travelled through " + typeList[0] + " deserts, " + typeList[1] + " forests, " + typeList[2] + " grasses, " + typeList[3] + " mountains, " + typeList[4] + " water ");

                float cost = typeList[0] * 5 + typeList[1] * 3 + typeList[2] * 1 + typeList[3] * 10;

                // 0 desert 1 forest 2 grass 3 mountain 4 water
                // 5 days   3 days   1 day   10 days    positive infinity
                Debug.Log("Total Cost: " + cost);

            }

        }

        // click mouse left
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseLeftRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit mouseLeftHit;

            if (Physics.Raycast(mouseLeftRay, out mouseLeftHit))
            {
                int index = System.Convert.ToInt32(mouseLeftHit.transform.name);
                if (path.Item1 == null)
                {
                    path.Item1 = globalNodes[index];
                }
                else
                {
                    path.Item2 = globalNodes[index];
                }
                mouseLeftHit.collider.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 1, 0));

            }

        }


    }


    /// <summary>
    /// 0 right 1 upper right 2 upper left 3 left 4 lower left 5 lower right
    /// </summary>
    private (int, int) offsetNeighbor(MyNode hex, int direction)
    {
        List<(int, int)> direction0 = new List<(int, int)>(new (int, int)[]
        {
            (1, 0), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)
        });
        List<(int, int)> direction1 = new List<(int, int)>(new (int, int)[]
        {
            (1, 0), (1, -1), (0, -1), (-1, 0), (0, 1), (1, 1)
        });

        List<List<(int, int)>> directions = new List<List<(int, int)>>();

        directions.Add(direction0);
        directions.Add(direction1);

        int parity = hex.y & 1;
        (int, int) dir = directions[parity][direction];
        return (hex.x + dir.Item1, hex.y + dir.Item2);

    }

    private void InitializeNeighbours(List<MyNode> myNodes)
    {
        for (int i1 = 0; i1 < globalNodes.Count; i1++)
        {
            MyNode node = this.globalNodes[i1];
            List<MyNode> temNodes = new List<MyNode>();
            // i < 6, 6 directions
            for (int i = 0; i < 6; i++)
            {
                (int, int) offset = offsetNeighbor(node, i);
                
                // boundary check
                if (offset.Item1 < 0 || offset.Item1 > mapSize - 1 || offset.Item2 < 0 || offset.Item2 > initialNodeCoordinate.Count - 1)
                {
                    continue;
                }
                else
                {
                    int index = offset.Item1 + offset.Item2 * mapSize;
                    temNodes.Add(globalNodes[index]);
                }
            }
            this.globalNodes[i1].Neighbours = temNodes;

        }

    }

}
