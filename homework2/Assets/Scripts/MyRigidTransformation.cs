using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Math;
using Accord.Math.Decompositions;
using System.Linq;
using Accord.MachineLearning;
using Accord.Statistics;


using Vector3 = UnityEngine.Vector3;
using Matrix4x4 = UnityEngine.Matrix4x4;

public class MyRigidTransformation : MonoBehaviour
{
    public List<Vector3> pointsSet1 = new List<Vector3>();
    public List<Vector3> pointsSet2 = new List<Vector3>();
    public GameObject PointCloud1;
    public GameObject PointCloud2;
    public GameObject pointPrefab;
    public GameObject linePrefab;
    public Material materialSet1;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Transform script started");
        Initialize();
        List<Vector3> pointsSet3 = new List<Vector3>();
        pointsSet3 = AlignPoints(pointsSet1, pointsSet2);
        VisualizePoints(pointsSet3, materialSet1);
        DrawMovementLines(pointsSet2, pointsSet3);
        Debug.Log("Transform script ended");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Initialize(){
        pointsSet1 = PointCloud1.GetComponent<PointCloudReader>().pointsSet1;
        pointsSet2 = PointCloud2.GetComponent<PointCloudReader>().pointsSet1;
    }

    private List<Vector3> MyRigidTransform(){
        Quaternion rotation = Quaternion.Euler(0, 0, 0); // rotation
        Vector3 translation = new Vector3(5, 0, 0); // translation
        Matrix4x4 rigidTransform = Matrix4x4.TRS(translation, rotation, Vector3.one);
        List<Vector3> transformedPointsSet1 = new List<Vector3>();

        foreach (Vector3 point in pointsSet2)
        {
            Vector3 transformedPoint = rigidTransform.MultiplyPoint3x4(point);
            transformedPointsSet1.Add(transformedPoint);
        }
        // Assign the transformed points to pointsSet2
        return transformedPointsSet1;
    }

    private List<Vector3> MyRigidTransform(Vector3 scale){
        Quaternion rotation = Quaternion.Euler(0, 0, 0); // rotation
        Vector3 translation = new Vector3(5, 0, 0); // translation
        
        // Apply the scale along with the rotation and translation
        Matrix4x4 rigidTransform = Matrix4x4.TRS(translation, rotation, scale);

        List<Vector3> transformedPointsSet1 = new List<Vector3>();

        foreach (Vector3 point in pointsSet1)
        {
            Vector3 transformedPoint = rigidTransform.MultiplyPoint3x4(point);
            transformedPointsSet1.Add(transformedPoint);
        }

        // Assign the transformed points to pointsSet2
        return transformedPointsSet1;
    }

    private void VisualizePoints(List<Vector3> points, Material material)
    {
        foreach (Vector3 point in points)
        {
            // Instantiate the prefab at each point's position
            GameObject pointObject = Instantiate(pointPrefab, point, Quaternion.identity);

            // Assign the material to the point
            Renderer pointRenderer = pointObject.GetComponent<Renderer>();
            if (pointRenderer != null)
            {
                pointRenderer.material = material;
            }
        }
    }

    private void DrawMovementLines(List<Vector3> originalPoints, List<Vector3> transformedPoints)
    {
        int minCount = Mathf.Min(originalPoints.Count, transformedPoints.Count);

        for (int i = 0; i < minCount; i++)
        {
            Vector3 start = originalPoints[i];
            Vector3 end = transformedPoints[i];

            // Instantiate the line renderer prefab
            GameObject lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            // Set line positions
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }

    private List<Vector3> AlignPoints(List<Vector3> sourcePoints, List<Vector3> targetPoints)
    {
        int minCount = Mathf.Min(sourcePoints.Count, targetPoints.Count);

        if (minCount < 3)
        {
            Debug.LogError("Point sets must have at least 3 points.");
            return null;
        }

        // Compute centroids
        Vector3 centroidSource = ComputeCentroid(sourcePoints.GetRange(0, minCount));
        Vector3 centroidTarget = ComputeCentroid(targetPoints.GetRange(0, minCount));

        // Center the points around the centroids
        List<Vector3> centeredSource = CenterPoints(sourcePoints.GetRange(0, minCount), centroidSource);
        List<Vector3> centeredTarget = CenterPoints(targetPoints.GetRange(0, minCount), centroidTarget);

        // Compute the rotation matrix using Kabsch algorithm
        Quaternion rotation = ComputeKabschRotation(centeredSource, centeredTarget);

        // Apply the rotation and translation to the smaller set of points
        List<Vector3> alignedPoints = new List<Vector3>();
        for (int i = 0; i < minCount; i++)
        {
            Vector3 point = sourcePoints[i];
            Vector3 rotatedPoint = rotation * (point - centroidSource);
            alignedPoints.Add(rotatedPoint + centroidTarget);
        }

        return alignedPoints;
    }

    private Vector3 ComputeCentroid(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum += point;
        }
        return sum / points.Count;
    }

    private List<Vector3> CenterPoints(List<Vector3> points, Vector3 centroid)
    {
        return points.Select(point => point - centroid).ToList();
    }

    private Quaternion ComputeKabschRotation(List<Vector3> P, List<Vector3> Q)
    {
        // Convert Vector3 lists to double[][] arrays
        double[][] PArray = P.Select(v => new double[] { v.x, v.y, v.z }).ToArray();
        double[][] QArray = Q.Select(v => new double[] { v.x, v.y, v.z }).ToArray();

        // Compute covariance matrix
        double[,] C = new double[3, 3];
        for (int i = 0; i < P.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    C[j, k] += PArray[i][j] * QArray[i][k];
                }
            }
        }

        // Compute the Singular Value Decomposition of C
        var svd = new Accord.Math.Decompositions.SingularValueDecomposition(C);

        // Compute rotation matrix
        double[,] R = Accord.Math.Matrix.Dot(svd.RightSingularVectors, svd.LeftSingularVectors.Transpose());

        // Convert rotation matrix to Matrix4x4
        Matrix4x4 RMatrix = new Matrix4x4();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                RMatrix[i, j] = (float)R[i, j];
            }
        }

        // Convert rotation matrix to quaternion
        return QuaternionFromMatrix(RMatrix);
    }

    private Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Convert a rotation matrix to a quaternion
        // This is a simplified method and may not handle all cases
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

}
