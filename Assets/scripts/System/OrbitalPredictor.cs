
using UnityEngine;

public class OrbitalPredictor
{
    public void compute_orbit(int seg, Vector3 center, float major, float minor, LineRenderer lr) //Disegna ellisse 
    {
        Vector3[] points = new Vector3[seg + 1];
        for (int i = 0; i < seg; i++)
        {
            float angle = ((float)i / (float)seg) * 360 * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * major + center.x;
            float y = Mathf.Cos(angle) * minor + center.y;
            points[i] = new Vector3(x, y, 0f);
        }
        points[seg] = points[0];
        lr.positionCount = seg + 1;
        lr.SetPositions(points);
    }

}
