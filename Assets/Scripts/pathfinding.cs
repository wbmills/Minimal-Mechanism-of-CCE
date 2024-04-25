using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Node
{
    public Vector3 pos;
    public Node parent;
    public float distanceFromStart = 0;
    public float distanceFromGoal = 0;
    public float totalDistance = 0;
    public bool alive = true;
}

public class pathfinding : MonoBehaviour
{
    public float step = 5f;
    public float maxDistanceUp = 3;
    public List<Node> nodes;
    public Node curNode;
    public Vector3 start;
    public Vector3 goal;
    public bool debug = false;
    public Node kingNode;
    private List<Vector3> dirs;
    public GameObject debugObject;
    private List<Node> usableNodes;
    private int iterations;
    private List<Vector3> search;
    private bool done;
    private int lastIt;

    // Start is called before the first frame update
    void Start()
    {
        lastIt = -1;
        StartCoroutine(searchReport());
        done = false;
        search = new List<Vector3>();
        ResetSearch();
        Search();
    }

    public void ResetSearch()
    {
        dirs = new List<Vector3>() {
            Vector3.right, Vector3.left,
            Vector3.forward, Vector3.back,
            Vector3.up, Vector3.down,
        };

        kingNode = null;
        curNode = new Node();
        curNode.pos = start;
        curNode.parent = null;
        curNode.distanceFromGoal = Vector3.Distance(curNode.pos, goal);
        curNode.distanceFromStart = 0;
        nodes = new List<Node>() { curNode };
        usableNodes = new List<Node>() { curNode };
    }

    // Update is called once per frame
    void Update()
    {
        if (search.Count != 0 && !done)
        {
            foreach (Vector3 pos in search)
            {
                Instantiate(debugObject, pos, Quaternion.identity);
            }
            var x = CalculateTotalDistance(search);
            print(x);
            done = true;
        }
        //print($"i: {iterations}\nNodes: {nodes.Count}\nUsable Nodes: {usableNodes.Count}");
    }

    public IEnumerator SearchLog()
    {
        print("Processing...");
        return null;
    }

    public IEnumerator searchReport()
    {
        yield return new WaitForSeconds(5);
        if (iterations != lastIt && search.Count == 0)
        {
            print("Still going...");
            lastIt = iterations;
            StartCoroutine(searchReport());
        }
        else
        {
            print("Done");
        }
    }

    public List<Vector3> Search()
    {
        iterations = 0;
        List<Vector3> final = new List<Vector3>();
        while (iterations < 10000000000 && nodes.Count < 50000000000 && kingNode == null && usableNodes.Count > 0)
        {
            curNode = GetShortestPath();
            if (AddNeighbors() == 0)
            {
                curNode.alive = false;
                usableNodes.Remove(curNode);
            }

            if (curNode.distanceFromGoal <= 2)
            {
                kingNode = curNode;
                while (kingNode.parent != null)
                {
                    final.Add(kingNode.pos);
                    kingNode = kingNode.parent;
                }
                print("done");
                search = final;
                return final;
            }
            iterations++;
        }
        return null;
    }

    public static float CalculateTotalDistance(List<Vector3> points)
    {
        float totalDistance = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            totalDistance += Distance(points[i], points[i + 1]);
        }
        return totalDistance;
    }

    public static float Distance(Vector3 p1, Vector3 p2)
    {
        float deltaX = p2.x - p1.x;
        float deltaY = p2.y - p1.y;
        float deltaZ = p2.z - p1.z;
        return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }

    // returns node with shortest total path from A to B
    private Node GetShortestPath()
    {
        Node min = null;
        foreach(Node cur in usableNodes)
        {
            if ((min == null && cur.alive) || (min != null && cur.totalDistance <= min.totalDistance && cur.alive))
            {
                min = cur;
            }
        }
        return min;
    }

    private List<Vector3> possibleNeighbors(Node n)
    {
        List<Vector3> possiblePositions = new List<Vector3>();
        Ray r;
        bool inArr = false;
        Physics.Raycast(n.pos, Vector3.down, out RaycastHit distanceConstraintRayhit, 10000);
        
        foreach (Vector3 dir in dirs)
        {
            inArr = false;

            r = new Ray(origin: n.pos, direction: dir);
            Physics.Raycast(origin: n.pos, direction: dir, out RaycastHit hit, step);
            Vector3 tempdir = new Vector3(dir.x, dir.y + Terrain.activeTerrain.terrainData.GetHeight(Mathf.RoundToInt(r.GetPoint(step).x), Mathf.RoundToInt(r.GetPoint(step).y)), dir.z);
            r.direction = tempdir;
            if ((distanceConstraintRayhit.collider != null && distanceConstraintRayhit.distance > maxDistanceUp))
            {
                inArr = true;
            }
            else if (hit.collider != null && hit.collider.tag != "Terrain")
            {
                inArr = true;
            }
            else
            {
                foreach (Node node in nodes)
                {
                    if (node.pos == r.GetPoint(step))
                    {
                        inArr = true;
                        break;
                    }
                }
            }

            if (!inArr)
            {
                possiblePositions.Add(r.GetPoint(step));
            }
        }

        return possiblePositions; 
    }

    public int AddNeighbors()
    {
        List<Vector3> neighbors = possibleNeighbors(curNode);
        foreach(Vector3 pos in neighbors)
        {
            Node newNode = new Node();
            newNode.parent = curNode;
            newNode.pos = new Vector3(
                pos.x, 
                pos.y, 
                pos.z);
            Instantiate(debugObject, newNode.pos, Quaternion.identity);
            newNode.distanceFromStart = curNode.distanceFromStart + step;
            newNode.distanceFromGoal = Vector3.Distance(newNode.pos, goal);
            newNode.totalDistance = newNode.distanceFromGoal + newNode.distanceFromStart;
            nodes.Add(newNode);
            usableNodes.Add(newNode);
        }
        return neighbors.Count;
    }
}
