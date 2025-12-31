using System.Collections.Generic;
using UnityEngine;

namespace PerspectiveRenderer.Grid
{
    public enum VanishingPoints
    {
        ONE_POINT,
        TWO_POINT,
        THREE_POINT
    }

    [ExecuteInEditMode]
    public class MasterRenderer : MonoBehaviour
    {

        [Header("Colors")]
        public Color colorFondo = Color.white;
        public Color colorLineas = Color.black;

        [Header("Horizon")]
        [Range(0f, 20f)]
        public float horizontAltitude = 0.5f;

        [Header("POV")]
        public VanishingPoints vanishing_points = VanishingPoints.TWO_POINT;

        [Range(-20f, 20f)]
        public float vanish_x = 0.5f;
        [Range(-20f, 20f)]
        public float vanish_y = 0.5f;
        [Range(-20f, 20f)]
        public float vanish_z = 0.5f;

        [Header("Grid")]
        [Range(-10f, 10f)]
        public float gridStartX = 0f;
        [Range(-10f, 10f)]
        public float gridEndX = 1f;
        [Range(2, 1000)]
        public int gridNumberX = 8;
        [Range(2, 1000)]
        public int gridNumberY = 8;
        [Range(0f, Mathf.PI/2)]
        public float gridAngle = 0f;

        public Material materialDibujo;

        void CreateMaterial()
        {
            if (!materialDibujo)
            {
                // Shader simple para dibujar colores planos sin verse afectado por la luz
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                materialDibujo = new Material(shader);
            }
        }

        // Se ejecuta después de que la cámara termina de renderizar
        void OnRenderObject()
        {
            GL.Clear(true, true, colorFondo);

            CreateMaterial();
            materialDibujo.SetPass(0);

            // 1. Dibujar el fondo blanco (un cuadrado que cubra toda la pantalla)
            GL.PushMatrix();
            GL.LoadOrtho(); // Usar coordenadas de 0 a 1 (pantalla)

            // 2. Dibujar las líneas de perspectiva
            GL.Begin(GL.LINES);
            GL.Color(colorLineas);

            switch(vanishing_points)
            {
                case VanishingPoints.ONE_POINT:
                    DrawOneVanishingPoint();
                    break;
                case VanishingPoints.TWO_POINT:
                    //DrawTwoVanishingPoints();
                    break;
                case VanishingPoints.THREE_POINT:
                    //DrawThreeVanishingPoints();
                    break;
            }
            
            GL.End();

            GL.PopMatrix();
        }

        void DrawOneVanishingPoint()
        {
            double factor = 1f / (gridNumberX-1);
            double width = gridEndX - gridStartX;
            double sideSize = factor * width;

            for (int i=0; i < gridNumberX; i++)
            {
                GL.Vertex3(vanish_x, horizontAltitude, 0);
                GL.Vertex3((float)(gridStartX + (i * sideSize)), 0f, 0);
            }

            double screenRatio = (double)Screen.width / (double)Screen.height;
            double prev_width = sideSize;
            double prev_y = 0d;

            for (int i=0; i < gridNumberY; i++)
            {
                double height = (prev_width * horizontAltitude) / (horizontAltitude + sideSize);
                
                //Debug.Log(i + " : height=" + height + ", prev_y=" + prev_y + ", prev_width=" + prev_width);
                GL.Vertex3(0, (float)(prev_y * screenRatio), 0);
                GL.Vertex3(1f, (float)(prev_y * screenRatio), 0);

                prev_y += height;
                prev_width = sideSize * (horizontAltitude - prev_y) / horizontAltitude;
            }
        }
    }
}