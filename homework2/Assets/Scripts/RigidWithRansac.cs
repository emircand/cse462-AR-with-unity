using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Accord.Math;
using Vector3 = UnityEngine.Vector3;
using Matrix4x4 = UnityEngine.Matrix4x4;

public class RigidWithRansac : MonoBehaviour
{
    public List<Vector3> pointsSet1 = new List<Vector3>();
    public List<Vector3> pointsSet2 = new List<Vector3>();
    public GameObject PointCloud1;
    public GameObject PointCloud2;
    public GameObject pointPrefab;
    public GameObject linePrefab; // Prefab with LineRenderer component
    public Material materialSet1;
    public int ransacIterations = 100;
    public float threshold = 1.0f;

    void Start()
    {
        Debug.Log("ransac script started");
        Initialize();
        List<Vector3> pointsSet3 = AlignPointsWithRansac(pointsSet2, pointsSet1);
        VisualizePoints(pointsSet3, materialSet1);
        DrawMovementLines(pointsSet2, pointsSet3);
        Debug.Log("ransac script ended");
    }

    void Initialize()
    {
        pointsSet1 = PointCloud1.GetComponent<PointCloudReader>().pointsSet1;
        pointsSet2 = PointCloud2.GetComponent<PointCloudReader>().pointsSet1;
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
        for (int i = 0; i < originalPoints.Count; i++)
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

    private List<Vector3> AlignPointsWithRansac(List<Vector3> sourcePoints, List<Vector3> targetPoints)
    {
        // Best transformation variables
        Quaternion bestRotation = Quaternion.identity;
        Vector3 bestTranslation = Vector3.zero;
        int bestInliers = 0;

        for (int i = 0; i < ransacIterations; i++)
        {
            // Randomly select a subset of points
            var sampleIndices = Enumerable.Range(0, sourcePoints.Count).OrderBy(x => Random.value).Take(3).ToArray();
            var sampledSourcePoints = sampleIndices.Select(index => sourcePoints[index]).ToList();
            var sampledTargetPoints = sampleIndices.Select(index => targetPoints[index]).ToList();
            // Compute transformation for this subset
            Quaternion rotation = ComputeKabschRotation(sampledSourcePoints, sampledTargetPoints);
            Vector3 translation = ComputeTranslation(sampledSourcePoints, sampledTargetPoints, rotation);
            // Count inliers
            int inliers = CountInliers(sourcePoints, targetPoints, rotation, translation, threshold);
            // Update best transformation if this one is better
            if (inliers > bestInliers)
            {
                bestInliers = inliers;
                bestRotation = rotation;
                bestTranslation = translation;
            }
        }
        // Apply the best transformation
        return sourcePoints.Select(p => bestRotation * p + bestTranslation).ToList();
    }

    private int CountInliers(List<Vector3> sourcePoints, List<Vector3> targetPoints, Quaternion rotation, Vector3 translation, float threshold)
    {
        int inliers = 0;
        for (int i = 0; i < sourcePoints.Count; i++)
        {
            Vector3 transformedPoint = rotation * sourcePoints[i] + translation;
            if (Vector3.Distance(transformedPoint, targetPoints[i]) < threshold)
            {
                inliers++;
            }
        }
        return inliers;
    }

    private Vector3 ComputeTranslation(List<Vector3> P, List<Vector3> Q, Quaternion rotation)
    {
        Vector3 centroidP = P.Aggregate(Vector3.zero, (acc, v) => acc + v) / P.Count;
        Vector3 centroidQ = Q.Aggregate(Vector3.zero, (acc, v) => acc + v) / Q.Count;

        // Compute the translation vector for the given rotation
        Vector3 translation = centroidQ - rotation * centroidP;

        return translation;
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
