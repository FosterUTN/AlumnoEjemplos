﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;

namespace AlumnoEjemplos.Los_Borbotones
{
    /// <summary>
    /// Herramienta para crear un SkyBox conformado por un cubo de 6 caras, cada cada con su
    /// propia textura.
    /// Luego de creado, si se modifica cualquier valor del mismo, se debe llamar a updateValues()
    /// para que tome efecto.
    /// </summary>
    public class CustomSkyBox : IRenderObject
    {
        /// <summary>
        /// Caras del SkyBox
        /// </summary>
        public enum SkyFaces
        {
            Up = 0,
            Down = 1,
            Front = 2,
            Back = 3,
            Right = 4,
            Left = 5,
        }


        private float skyEpsilon;
        /// <summary>
        /// Valor de desplazamiento utilizado para que las caras del SkyBox encajen bien entre sí.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float SkyEpsilon
        {
            get { return skyEpsilon; }
            set { skyEpsilon = value; }
        }

        private Vector3 size;
        /// <summary>
        /// Tamaño del SkyBox.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        private Vector3 center;
        /// <summary>
        /// Centro del SkyBox.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        private Color color;
        /// <summary>
        /// Color del SkyBox.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }


        private TgcMesh[] faces;
        private Matrix[] matrices;
        /// <summary>
        /// Meshes de cada una de las 6 caras del cubo, en el orden en que se enumeran en SkyFaces
        /// </summary>
        public TgcMesh[] Faces
        {
            get { return faces; }
        }


        private string[] faceTextures;
        /// <summary>
        /// Path de las texturas de cada una de las 6 caras del cubo, en el orden en que se enumeran en SkyFaces
        /// </summary>
        public string[] FaceTextures
        {
            get { return faceTextures; }
        }

        private bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return faces[0].AlphaBlendEnable; }
            set
            {
                foreach (TgcMesh face in faces)
                {
                    face.AlphaBlendEnable = true;
                }
            }
        }

        /// <summary>
        /// Crear un SkyBox vacio
        /// </summary>
        public CustomSkyBox()
        {
            faces = new TgcMesh[6];
            matrices = new Matrix[6];
            faceTextures = new string[6];
            skyEpsilon = 5f;
            color = Color.White;
            center = new Vector3(0, 0, 0);
            size = new Vector3(1000, 1000, 1000);
            alphaBlendEnable = false;
        }

        /// <summary>
        /// Configurar la textura de una cara del SkyBox
        /// </summary>
        /// <param name="face">Cara del SkyBox</param>
        /// <param name="texturePath">Path de la textura</param>
        public void setFaceTexture(SkyFaces face, string texturePath)
        {
            faceTextures[(int)face] = texturePath;
        }

        /// <summary>
        /// Renderizar SkyBox
        /// </summary>
        public void render()
        {
            foreach (TgcMesh face in faces)
            {
                face.render();
            }
        }

        /// <summary>
        /// Liberar recursos del SkyBox
        /// </summary>
        public void dispose()
        {
            foreach (TgcMesh face in faces)
            {
                face.dispose();
            }
        }

        public void transform(Matrix matrix)
        {
            for (int i = 0; i < faces.Length; i++ )
            {
                faces[i].Transform = matrix * matrices[i];
            }
        }

        /// <summary>
        /// Tomar los valores configurados y crear el SkyBox
        /// </summary>
        public void updateValues()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear cada cara
            for (int i = 0; i < faces.Length; i++)
            {
                //Crear mesh de D3D
                Mesh m = new Mesh(2, 4, MeshFlags.Managed, TgcSceneLoader.DiffuseMapVertexElements, d3dDevice);
                SkyFaces skyFace = (SkyFaces)i;

                // Cargo los vértices
                using (VertexBuffer vb = m.VertexBuffer)
                {
                    GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                    int colorRgb = this.color.ToArgb();
                    cargarVertices(skyFace, data, colorRgb);
                    vb.Unlock();
                }

                // Cargo los índices
                using (IndexBuffer ib = m.IndexBuffer)
                {
                    short[] ibArray = new short[6];
                    cargarIndices(ibArray);
                    ib.SetData(ibArray, 0, LockFlags.None);
                }

                //Crear TgcMesh
                string faceName = Enum.GetName(typeof(SkyFaces), skyFace);
                TgcMesh faceMesh = new TgcMesh(m, "SkyBox-" + faceName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
                faceMesh.Materials = new Material[] { TgcD3dDevice.DEFAULT_MATERIAL };
                faceMesh.createBoundingBox();
                faceMesh.Enabled = true;

                //textura
                TgcTexture texture = TgcTexture.createTexture(d3dDevice, faceTextures[i]);
                faceMesh.DiffuseMaps = new TgcTexture[] { texture };

                faceMesh.AutoTransformEnable = false;
                faces[i] = faceMesh;
                matrices[i] = faceMesh.Transform;
            }
        }


        /// <summary>
        /// Crear Vertices segun la cara pedida
        /// </summary>
        private void cargarVertices(SkyFaces face, GraphicsStream data, int color)
        {
            switch (face)
            {
                case SkyFaces.Up: cargarVerticesUp(data, color); break;
                case SkyFaces.Down: cargarVerticesDown(data, color); break;
                case SkyFaces.Front: cargarVerticesFront(data, color); break;
                case SkyFaces.Back: cargarVerticesBack(data, color); break;
                case SkyFaces.Right: cargarVerticesRight(data, color); break;
                case SkyFaces.Left: cargarVerticesLeft(data, color); break;
            }
        }

        /// <summary>
        /// Crear vertices para la cara Up
        /// </summary>
        private void cargarVerticesUp(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, 1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y + size.Y / 2,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y + size.Y / 2,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y + size.Y / 2,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y + size.Y / 2,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);
        }

        /// <summary>
        /// Crear vertices para la cara Down
        /// </summary>
        private void cargarVerticesDown(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, -1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y - size.Y / 2,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y - size.Y / 2,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y - size.Y / 2,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y - size.Y / 2,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        /// Crear vertices para la cara Front
        /// </summary>
        private void cargarVerticesFront(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, -1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z + size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z + size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z + size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z + size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        /// Crear vertices para la cara Back
        /// </summary>
        private void cargarVerticesBack(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, -1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z - size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2 + skyEpsilon,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z - size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z - size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2 - skyEpsilon,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z - size.Z / 2
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        /// Crear vertices para la cara Right
        /// </summary>
        private void cargarVerticesRight(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, -1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X + size.X / 2,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        /// Crear vertices para la cara Left
        /// </summary>
        private void cargarVerticesLeft(GraphicsStream data, int color)
        {
            TgcSceneLoader.DiffuseMapVertex v;
            Vector3 n = new Vector3(0, -1, 0);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 0;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z - size.Z / 2 - skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 0;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2,
                center.Y - size.Y / 2 - skyEpsilon,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 1;
            data.Write(v);

            v = new TgcSceneLoader.DiffuseMapVertex();
            v.Position = new Vector3(
                center.X - size.X / 2,
                center.Y + size.Y / 2 + skyEpsilon,
                center.Z + size.Z / 2 + skyEpsilon
                );
            v.Normal = n;
            v.Color = color;
            v.Tu = 1;
            v.Tv = 0;
            data.Write(v);
        }

        /// <summary>
        /// Generar array de indices
        /// </summary>
        private void cargarIndices(short[] ibArray)
        {
            int i = 0;
            ibArray[i++] = 0;
            ibArray[i++] = 1;
            ibArray[i++] = 2;
            ibArray[i++] = 0;
            ibArray[i++] = 2;
            ibArray[i++] = 3;
        }

    }
}
