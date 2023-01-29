using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformInterpolator : MonoBehaviour
{
    public List<movementLine> edges = new List<movementLine>();
    protected int index = 0;
    protected float T = -1f;
    public float delayedStart = 0f;
    protected float secondsElapsed = 0f;
    public movementType movement;
    public edgeType interpolationType;
    public bool canMove = false;
    float distance = 0f;

    public float speed = -1f;
    MotionTable table;
   
public int stopSpot = 0;
    Entry comingFrom;
    Entry goingTo;

    /*
    private void OnDrawGizmos()
    {
        Entry prev = null;
        foreach(Entry e in table.getEntries())
        {
            if(prev != null)
            {
                Gizmos.DrawLine(prev.getPosition(), e.getPosition());
                Gizmos.DrawSphere(e.getPosition(),0.1f);
            }
            prev = e;
           
        }
    }
    */
    public void setTransform(Transform r)
    {
        transform.localScale = r.localScale;
        transform.rotation = r.rotation;
        transform.position = r.position;
    }

    public void startMove()
    {
        T = 0f;
        canMove = true;
        secondsElapsed = 0f;
        if (speed >= 0f)
        {
            //make Motion table

            distance = 0;
            table = new MotionTable(edges, interpolationType);
            comingFrom = table.getAtIndex(0);
            goingTo = table.getAtIndex(1);
            //Debug.Log(comingFrom);
           // Debug.Log(goingTo);
        }
    }

    protected float getNewT()
    {
        return getTByType(movement);
    }

    private float getTByType(movementType type)
    {
        if (type == movementType.LERP)
        {
            return T;
        }
        if (type == movementType.EASEIN)
        {
            return 1 - Mathf.Cos((T * Mathf.PI) / 2);
        }
        if (type == movementType.EASEOUT)
        {
            return Mathf.Sin((T * Mathf.PI) / 2);
        }
        if (type == movementType.EASEALL)
        {
            if (index == 0)
            {
                return getTByType(movementType.EASEIN);
            }
            else if (index == edges.Count-1)
            {
                return getTByType(movementType.EASEOUT);
            }
            else
            {
                return getTByType(movementType.LERP);
            }
        }
        return T;
    }


    private void addDistance()
    {
        if (speed >= 0f)
        {
            float finalSpeed = speed;
            if (comingFrom.getSpeed() >= 0)
            {
                finalSpeed = comingFrom.getSpeed();
            }
            distance += (finalSpeed * Time.deltaTime);

            //Debug.Log(comingFrom.getDistance());
            if (goingTo.getIndex() == table.getEntries().Count-1)
            {
                //setTransform(edges[index].endTrans);
                T = -1;
                canMove = false;
                index = 0;
                secondsElapsed = -1f;
                
                return;
            }
            comingFrom = goingTo;
            goingTo = table.getEntryUsingDistance(distance);
            //Debug.Log("INDEX " + goingTo.getIndex());
            transform.position = goingTo.getPosition();
            transform.rotation = goingTo.getRotation();
            transform.localScale = goingTo.getScale();

            //set rotation
            //Debug.Log(distance);
            return;
        }
    }

    private void changePosition(float t)
    {
        
        
        if (interpolationType == edgeType.BEZIER)
        {
            int indexBehind =(index - 1);
            int indexAhead = (index + 1);
            int indexAhead2 = (index + 1);
            if(indexAhead >= edges.Count)
            {
                indexAhead = edges.Count - 1;
            }
            if(indexAhead2 >= edges.Count)
            {
                indexAhead2 = edges.Count - 1;
            }
            if(indexBehind < 0)
            {
                indexBehind = 0;
            }
            transform.position = maths.Bezier(edges[indexBehind].transform.position, edges[index].transform.position,  edges[indexAhead].transform.position, edges[indexAhead2].transform.position, t);

        }
        else if(interpolationType == edgeType.CATMULL)
        {
            int indexBehind = maths.nfmod((index - 1), edges.Count);
            if (indexBehind < 0)
            {
                indexBehind = 0;
            }
            int indexAhead = maths.nfmod((index + 1), edges.Count);
            int indexAhead2 = maths.nfmod((index + 2), edges.Count);
            transform.position = maths.Catmull(edges[indexBehind].transform.position, edges[index].transform.position, edges[indexAhead].transform.position, edges[indexAhead2].transform.position, t);

        }
        else
        {
            int indexAhead = (index + 1);
            
            if (indexAhead >= edges.Count)
            {
                indexAhead = edges.Count - 1;
            }
            transform.position = Vector3.Lerp(edges[index].transform.position, edges[indexAhead].transform.position, t);

        }

    }



    protected void move()
    {
        if (!canMove)
        {
            return;
        }
        if (secondsElapsed >= 0f)
        {
            secondsElapsed += Time.deltaTime;
            if (secondsElapsed <= delayedStart)
            {
                return;
            }

        }
        if(speed >= 0)
        {
            addDistance();
            return;
        }
        if (T >= 0 && speed < 0)
        {
           // Debug.Log("T: " + T.ToString() + " INDEX" + index.ToString());
            T += Time.deltaTime / edges[index].totalTime;
            float newT = getNewT();
            int indexAhead = (index + 1);
            
            if (indexAhead >= edges.Count)
            {
                indexAhead = edges.Count - 1;
            }
            transform.localScale = Vector3.Lerp(edges[index].transform.localScale, edges[indexAhead].transform.localScale, newT);
            transform.rotation = Quaternion.Slerp(edges[index].transform.rotation, edges[indexAhead].transform.rotation, newT);
            changePosition(newT);
            if (T > 1 && speed < 0)
            {
                if((index) < (edges.Count-1))
                {
                   // Debug.Log("ENOUGH ROOM");
                    index++;
                    //setTransform(edges[index].startTrans);
                    T = 0;
                }
                else
                {
                    setTransform(edges[index].transform);
                    T = -1;
                    canMove = false;
                    index = 0;
                    secondsElapsed = -1f;
                    return;
                }
                
            }
        }
    }
    
}

public enum movementType
{
    LERP,
    EASEIN,
    EASEOUT,
    EASEALL
}

public enum edgeType
{
    LERP,
    CATMULL,
    BEZIER
}

[System.Serializable]
public class movementLine
{
    [SerializeField] public Transform transform;
    [SerializeField] public float totalTime = 1f;
    [SerializeField] public float arrivalSpeed = -1f;
}
public class MotionTable
{
    List<Entry>  entries = new List<Entry>();

    Transform lastIndexedTransform;
    Transform nextIndexedTransform;
    int lastTransformIndex = 0;
    int nextTransformIndex = 1;
    public MotionTable(List<movementLine> edges, edgeType type)
{
        List<Vector3> a = new List<Vector3>();
        movementLine lastLine = null;
        foreach (movementLine line in edges)
        {
            a.Add(line.transform.position);
            lastLine = line;

        }
        
        

        int samples = 20;//how many individual LERPS you want each spline (of 4 points) to be split into
    float u = 0f;//used for knowing which section of the curve is being modified (ignore)

        int index = 0;

    //indices of the points used
    int p0 = a.Count - 1;//control
    int p1 = 0;//waypoint
    int p2 = 1;//waypoint
    int p3 = 2;//control

    for (float i = 0; i < (a.Count-1);)
    {
        //increase the index of the vertex points (the points at the end of the curve) (not control points)
        if (u >= (1f-(1f/samples)))
        {//u could be replaced with i, but its ok
            u = 0f;
            p1++;
            p2++;

                nextTransformIndex++;
                lastTransformIndex++;
            }

        if(nextTransformIndex >= edges.Count)
            {
                nextTransformIndex = edges.Count - 1;
            }

            if (lastTransformIndex >= edges.Count)
            {
                lastTransformIndex = edges.Count - 1;
            }
            //Debug.Log(nextTransformIndex);
            lastIndexedTransform = edges[lastTransformIndex].transform;
            nextIndexedTransform = edges[nextTransformIndex].transform;
            //set the indexes of the control points (computed on the fly) (i realize that i might not need to do this, but it works)
            p0 = p1 - 1;
p3 = p2 + 1;
//if at the end, set the point's position to just be the last point in the list of vertices
if (p0 >= a.Count)
{
    p0 = a.Count - 1;
}
if (p1 >= a.Count)
{
    p1 = a.Count - 1;
}
if (p2 >= a.Count)
{
    p2 = a.Count - 1;
}
if (p3 >= a.Count)
{
    p3 = a.Count - 1;

}
            if(p0 < 0)
            {
                p0 = 0;
            }


            // Debug.Log(p0.ToString() + " " + p1.ToString() + " " + p2.ToString() + " " + p3.ToString() + " ");
            //calculate where the point in world space is using T and the curve points (from the list of points at the indexes of p0-p4)
            Vector3 currentPos = Vector3.zero;
            if (type == edgeType.BEZIER)
            {
                 currentPos = maths.Bezier(a[p0], a[p1], a[p2], a[p3], u);
            }
            if (type == edgeType.CATMULL)
            {
                currentPos = maths.Catmull(a[p0], a[p1], a[p2], a[p3], u);
            }
            if (type == edgeType.LERP)
            {
                currentPos = Vector3.Lerp(a[p0], a[p1], u);
            }

            

            //Debug.Log(index.ToString() + " " + u.ToString());
            
            Quaternion rot = Quaternion.Slerp(lastIndexedTransform.rotation, nextIndexedTransform.rotation, u);
            Vector3 scale = Vector3.Lerp(lastIndexedTransform.localScale, nextIndexedTransform.localScale, u);
            float speed = Mathf.Lerp(edges[lastTransformIndex].arrivalSpeed, edges[nextTransformIndex].arrivalSpeed, u);

            //Debug.Log(rot);
            //Debug.Log(scale);
            //Debug.Log(currentPos);
           // Debug.Log(index.ToString() + " " + ((samples * a.Count) - 1).ToString());
            Entry lastEntry = null;

if(index == ((samples* a.Count) - 1))
            {
                Entry previous = entries[entries.Count - 1];
                lastEntry = new Entry(i, (Vector3.Distance(currentPos, previous.getPosition()) + previous.getDistance()), edges[edges.Count-1].transform.position, index, edges[edges.Count - 1].transform.rotation, edges[edges.Count - 1].transform.localScale, speed);
                Debug.Log("LAST");
                entries.Add(lastEntry);
            }
            else if (entries.Count >= 1)
            {//if the list is empty, just add the first point at T=0 on the curve (or the first point you gave it)
                Entry previous = entries[entries.Count - 1];

                lastEntry = new Entry(i, (Vector3.Distance(currentPos, previous.getPosition()) + previous.getDistance()), currentPos, index, rot, scale, speed);

                entries.Add(lastEntry);
                //entry is a custom class that stores T, the distance along the curve, and the position in worldspace
            }
            else
{

                //add entry to the lsit
                lastEntry = new Entry(i, 0, a[0], 0, lastIndexedTransform.rotation, lastIndexedTransform.localScale, edges[0].arrivalSpeed);
    entries.Add(lastEntry);
}
            
            //Debug.Log(rot);
            //lastEntry.setRotationAndScale(rot, scale);

            //increment i by 1/samples so by the time i=(size of list), there will be samples*list.size entries in the list of entries
            i += (1f / (samples));
//u is used to check when to increment p1 & p2
u += (1f / (samples));

            index++;
    }
        //entries.RemoveAt(entries.Count - 2);
    //std::cout << "Motion table created with length of: " << entries.size() << std::endl;
}

    public Entry getEntryUsingT(float u)
    {
        Entry previousEntry = entries[0];
        Entry currentEntry = entries[0];
        float percent = 0f;
        foreach(Entry e in entries) {
            currentEntry = e;
            if (previousEntry.getT() <= u && u <= currentEntry.getT())
            {
                break;
            }
            previousEntry = e;
        }
        percent = (u + previousEntry.getT()) / currentEntry.getT();
        return new Entry(Mathf.Lerp(previousEntry.getT(), currentEntry.getT(), percent),
            Mathf.Lerp(previousEntry.getDistance(), currentEntry.getDistance(), percent),
            Vector3.Lerp(previousEntry.getPosition(), currentEntry.getPosition(), percent),previousEntry.getIndex(),
            Quaternion.Slerp(previousEntry.getRotation(),currentEntry.getRotation(),percent),
            Vector3.Lerp(previousEntry.getScale(), currentEntry.getScale(),percent),
             Mathf.Lerp(previousEntry.getSpeed(), currentEntry.getSpeed(), percent));
    }

   public Entry getEntryUsingDistance(float d)
    {
        if(d == 0)
        {
            return entries[0];
        }
        Entry previousEntry = entries[0];
        Entry currentEntry = entries[0];
        float percent = 0f;
        foreach(Entry e in entries) {
            currentEntry = e;
            //std::cout << "PREV " << previousEntry.getDistance() << " CURR " << currentEntry.getDistance() << std::endl;
            if (previousEntry.getDistance() <= d && d <= currentEntry.getDistance())
            {
                break;
            }
            previousEntry = e;
        }
        percent = (d - previousEntry.getDistance()) / (currentEntry.getDistance() - previousEntry.getDistance());
        //Debug.Log(percent);
        return new Entry(Mathf.Lerp(previousEntry.getT(), currentEntry.getT(), percent),
            Mathf.Lerp(previousEntry.getDistance(), currentEntry.getDistance(), percent),
            Vector3.Lerp(previousEntry.getPosition(), currentEntry.getPosition(), percent), previousEntry.getIndex(),
            Quaternion.Slerp(previousEntry.getRotation(), currentEntry.getRotation(), percent),
            Vector3.Lerp(previousEntry.getScale(), currentEntry.getScale(), percent),
             Mathf.Lerp(previousEntry.getSpeed(), currentEntry.getSpeed(), percent));
    }

    public Entry getEntryUsingPoint(Vector3 p)
    {
        Entry previousEntry = entries[0];
        Entry currentEntry = entries[0];
        float percent = 0f;
        foreach(Entry e in entries) {
            currentEntry = e;
            if (previousEntry.getPosition().x <= p.x && p.x <= currentEntry.getPosition().x)
            {
                if (previousEntry.getPosition().y <= p.y && p.y <= currentEntry.getPosition().y)
                {
                    if (previousEntry.getPosition().z <= p.z && p.z <= currentEntry.getPosition().z)
                    {
                        break;
                    }
                }

            }
            previousEntry = e;
        }
        percent = (p.x + previousEntry.getPosition().x) / currentEntry.getPosition().x;
        return new Entry(Mathf.Lerp(previousEntry.getT(), currentEntry.getT(), percent),
            Mathf.Lerp(previousEntry.getDistance(), currentEntry.getDistance(), percent),
            Vector3.Lerp(previousEntry.getPosition(), currentEntry.getPosition(), percent), previousEntry.getIndex(),
            Quaternion.Slerp(previousEntry.getRotation(), currentEntry.getRotation(), percent),
            Vector3.Lerp(previousEntry.getScale(), currentEntry.getScale(), percent),
             Mathf.Lerp(previousEntry.getSpeed(), currentEntry.getSpeed(), percent));
    }

    public Entry getAtIndex(int i)
    {
        return entries[i];
    }

    public List<Entry> getEntries()
    {
        return entries;
    }
}

public class Entry
{
    float u;
    float distance;//distance along curve, not from origin
    Vector3 position;
    Quaternion rotation;
    Vector3 scale;
    int index;
    float speed;
    public Entry(float a, float b, Vector3 c, int i, Quaternion q, Vector3 s, float spe) {
	u = a;
	distance = b;
	position = c;
        rotation = q;
        scale = s;
        speed = spe;
        index = i;
}

    public float getSpeed()
    {
        return speed;
    }
    public void setRotationAndScale(Quaternion q, Vector3 s)
    {
        rotation = q;
        scale = s;
    }
    public Vector3 getScale()
    {
        return scale;
    }
    public Quaternion getRotation()
    {
        return rotation;
    }
    public int getIndex()
    {
        return index;
    }
   public float getT()
    {
        return u;
    }

   public float getDistance()
    {
        return distance;
    }

   public Vector3 getPosition()
    {
        return position;
    }

}

