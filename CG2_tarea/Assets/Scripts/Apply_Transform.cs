/*
Script that will be applied to the car

Mariel Gómez Gutiérrez A01275607
2023-11-13
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apply_Transform : MonoBehaviour
{
    [SerializeField] Vector3[] points;
    [SerializeField] float angleSpin = -180f;
    [SerializeField] float speed = 1f;
    [SerializeField] float scale = 0.21f;
    [SerializeField] GameObject wheel;
    [SerializeField] AXIS rotationAxis;
    GameObject[] wheels = new GameObject[4];
    float width = 0.52f;
    float timepoint = 0f;
    int cpoint = 0;

    //Wheels
    // Vector3 wheelPositionsF = new Vector3(0.52f, 0.19f, 0.88f);
    // Vector3 wheelPositionsB = new Vector3(0.52f, 0.19f, -0.88f);
    // Vector3 wheelPositionsF1 = new Vector3(-0.52f, 0.19f, 0.88f);
    // Vector3 wheelPositionsB1 = new Vector3(-0.52f, 0.19f, -0.88f);
    [SerializeField] Vector3[] wheelPositions;

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] newVertices;

    Mesh[] wheelMesh = new Mesh[4];
    Vector3[][] baseVerticesWheel = new Vector3[4][];
    Vector3[][] newVerticesWheel = new Vector3[4][];

    // Start is called before the first frame update
    void Start()
    {
        points[0] = new Vector3(0, 0, 0);

        //Creating the wheels
        for (int i = 0; i < 4; i++){
            wheels[i] = Instantiate(wheel, wheelPositions[i], Quaternion.identity);
        }

        //CAR
        mesh = GetComponentInChildren<MeshFilter>().mesh; //obtener el componenet mesh del hijo del objeto
        baseVertices = mesh.vertices; //obtener la lista de los vertices del mesh, just a backup
        newVertices = new Vector3[baseVertices.Length]; //crear un nuevo arreglo de vectores con la misma longitud que el arreglo de vertices originales
        for (int i=0; i<baseVertices.Length; i++) //recorrer la lista de vertices
        {
            newVertices[i] = baseVertices[i]; //hacemos la copia de los originales
        }

        //Wheel
        for (int i = 0; i < 4; i++)
        {
            wheelMesh[i] = wheels[i].GetComponentInChildren<MeshFilter>().mesh;
            baseVerticesWheel[i] = wheelMesh[i].vertices;
            newVerticesWheel[i] = new Vector3[baseVerticesWheel[i].Length];
            for (int j = 0; j < baseVerticesWheel[i].Length; j++)
            {
                newVerticesWheel[i][j] = baseVerticesWheel[i][j];
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        timepoint += Time.deltaTime;
        if (timepoint >= speed)
        {
            timepoint = 0;
            cpoint++;
            if (cpoint >= points.Length)
            {
                cpoint = 0;
            }
        }

        DoTransform();
    }

    void DoTransform()
    {
        Vector3 direction = Vector3.Lerp(points[cpoint], points[(cpoint + 1) % points.Length], timepoint / speed);

        Matrix4x4 move = HW_Transforms.TranslationMat(direction.x, 
                                                      direction.y,
                                                      direction.z);
        
        Matrix4x4 moveOrigin = HW_Transforms.TranslationMat(0, 0, 0);

        Vector3 goingto = points[(cpoint + 1) % points.Length] - points[cpoint];
        float angle = Vector3.SignedAngle(transform.forward, goingto, Vector3.down);
        Matrix4x4 rotate = HW_Transforms.RotateMat(angle, AXIS.Y);
        Matrix4x4 composite =  move * rotate;

        // Matrix4x4 composite = moveObject * rotate * moveOrigin;

        for (int i=0; i<newVertices.Length; i++)
        {
            //Necesitamos un vector4 para poder multiplicarlo por la matriz
            Vector4 temp = new Vector4(baseVertices[i].x, 
                                       baseVertices[i].y, 
                                       baseVertices[i].z, 
                                       1.0f);

            newVertices[i] = composite * temp; //Importa el orden, entonces primero la matriz y luego el vector
        }

        //Asignar los nuevos vertices al mesh 
        mesh.vertices = newVertices;
        mesh.RecalculateNormals();

        Matrix4x4 scaleMat = HW_Transforms.ScaleMat(scale, scale, scale);
        Matrix4x4 spin = HW_Transforms.RotateMat(angleSpin * Time.time, AXIS.X);
        Matrix4x4 spinComp = spin * scaleMat;
        //composite = move * spin * scaleMat;


        for (int i = 0; i < 4; i++)
        {
            Matrix4x4 pivot = HW_Transforms.TranslationMat(wheelPositions[i].x, wheelPositions[i].y, wheelPositions[i].z);
            Matrix4x4 pivotBack = HW_Transforms.TranslationMat(-wheelPositions[i].x, -wheelPositions[i].y, -wheelPositions[i].z);

            for (int j = 0; j < baseVerticesWheel[i].Length; j++)
            {
                Vector4 tmp = new Vector4(baseVerticesWheel[i][j].x, baseVerticesWheel[i][j].y, baseVerticesWheel[i][j].z, 1);

                newVerticesWheel[i][j] = move * pivotBack * rotate * pivot * spinComp * tmp;
            }

            wheelMesh[i].vertices = newVerticesWheel[i];
            wheelMesh[i].RecalculateNormals();
        }

    }
}
