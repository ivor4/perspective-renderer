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
                    DrawTwoVanishingPoint();
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
            double factor = 1f / (gridNumberX - 1);
            double width = gridEndX - gridStartX;
            double sideSize = factor * width;
            double screenRatio = (double)Screen.width / (double)Screen.height;

            

            
            double prev_y = 0d;
            double highest_y = 0d;

            for (int i=0; i < gridNumberY; i++)
            {
                double new_y = horizontAltitude * ((sideSize * (horizontAltitude - prev_y)) + (prev_y * horizontAltitude));
                new_y /= (sideSize * (horizontAltitude - prev_y)) + (horizontAltitude * horizontAltitude);

                double delta_y = new_y - prev_y;

                if (delta_y < 0.001d)
                {
                    break;
                }

                highest_y = new_y;

                DrawLineCorrectingFactor(0f, (float)prev_y, 1f, (float)prev_y, (float)screenRatio);

                prev_y = new_y;
            }

            for (int i = 0; i < gridNumberX; i++)
            {
                double finalX = gridStartX + (i * sideSize);
                double growXFactor = finalX - vanish_x;
                growXFactor /= horizontAltitude;
                double initialX = (horizontAltitude - highest_y) * growXFactor + vanish_x;
                DrawLineCorrectingFactor((float)initialX, (float)highest_y, (float)(gridStartX + (i * sideSize)), 0f, (float)screenRatio);
            }
        }

        void DrawTwoVanishingPoint()
        {
            double factor = 1f / (gridNumberX - 1);
            double width = gridEndX - gridStartX;
            double sideSize = factor * width;
            double screenRatio = (double)Screen.width / (double)Screen.height;


            for (int i = 0; i < gridNumberX; i++)
            {
                DrawLineCorrectingFactor(vanish_x, horizontAltitude, (float)(gridStartX + (i * sideSize)), 0f, (float)screenRatio);
            }

            for (int i = 0; i < gridNumberY; i++)
            {
                DrawLineCorrectingFactor(vanish_y, horizontAltitude, (float)(gridStartX + (i * sideSize)), 0f, (float)screenRatio);
            }
        }

        static void DrawLineCorrectingFactor(float x1, float y1, float x2, float y2, float factor)
        {
            float correctedX1 = x1;
            float correctedY1 = y1 * factor;
            float correctedX2 = x2;
            float correctedY2 = y2 * factor;

            GL.Vertex3(correctedX1, correctedY1, 0);
            GL.Vertex3(correctedX2, correctedY2, 0);
        }
    }
}