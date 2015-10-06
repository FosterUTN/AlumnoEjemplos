﻿using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;


namespace AlumnoEjemplos.Los_Borbotones
{
    public abstract class Enemy : GameObject
    {
        public float health;
        public float score;
        public int attackDamage;
        public bool attacking = false;
        public bool attacked = false;
        public float ATTACK_RANGE = 200f;
        public float attackDelay = 0;
        public float MOVEMENT_SPEED = 125f;
        public float SPAWN_RADIUS= 3000f;
        public Matrix posicionActual;
        public Vector3 Normal;
        public float MESH_SCALE;
        public Vector3 vectorDireccion;
        public Vector3 vectorDireccionRotacion;
        Device d3dDevice = GuiController.Instance.D3dDevice;
        public float SPAWN_HEIGHT = 0;
        public  Matrix giroInicial;
        public TgcBoundingBox HEADSHOT_BOUNDINGBOX;
        public TgcBoundingBox CHEST_BOUNDINGBOX;
        public TgcBoundingBox LEGS_BOUNDINGBOX;
        public Matrix posicionActualHeadshot;
        public Matrix posicionActualChest;
        public Matrix posicionActualLegs;
        public Matrix Traslacion;
        public Matrix MatOrientarObjeto;
        public Matrix posicionAnterior;
        public Matrix posicionAnteriorHeadshot;
        public Matrix posicionAnteriorChest;
        public Matrix posicionAnteriorLegs;
        public Vector3 vectorDireccionAnterior;
        public Vector3 vectorDireccionRotacionAnterior;
        public Tgc3dSound SonidoMovimiento ;

        public override void Init()   
        {
            mesh.AutoTransformEnable = false;

            mesh.Transform = CreatorMatrixPosition();

            mesh.BoundingBox.transform(CreatorMatrixPosition());

            Matrix matt = Matrix.Translation(new Vector3(mesh.Transform.M41, mesh.Transform.M42, mesh.Transform.M43));
            Matrix matScale = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            this.posicionActual = matScale * giroInicial * matt;

            setBaseEffect();

        }

        public override void Update(float elapsedTime)
        {
            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);

            if (!attacking)
            {
                vectorDireccion = (CustomFpsCamera.Instance.Position - vectorPosActual);
                if (vectorDireccion.Length() <= ATTACK_RANGE && attackDelay <= 0) 
                { 
                    startAttack();
                }
                vectorDireccionRotacion = new Vector3(vectorDireccion.X, 0, vectorDireccion.Z);
                vectorDireccionRotacion.Normalize();

                vectorDireccion.Normalize();

                attackDelay -= elapsedTime;
            }
            
            updateMovementMatrix(elapsedTime, vectorDireccion);

            //Colision con Player1
            TgcCollisionUtils.BoxBoxResult resultPlayer = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, CustomFpsCamera.Instance.boundingBox);
            if (resultPlayer == TgcCollisionUtils.BoxBoxResult.Adentro || resultPlayer == TgcCollisionUtils.BoxBoxResult.Atravesando)
            {
                attack(elapsedTime);
            }
            
            //Colision con otros enemigos
            foreach (Enemy enemy in GameManager.Instance.enemies)
            {
                if (this != enemy && TgcCollisionUtils.testAABBAABB(this.mesh.BoundingBox, enemy.mesh.BoundingBox))
                {
                    Vector3 eye = CustomFpsCamera.Instance.Position;

                    Vector3 vec1 = new Vector3();
                    vec1.X = this.posicionActual.M41;
                    vec1.Y = this.posicionActual.M42;
                    vec1.Z = this.posicionActual.M43;

                    Vector3 vec2 = new Vector3();
                    vec2.X = enemy.posicionActual.M41;
                    vec2.Y = enemy.posicionActual.M42;
                    vec2.Z = enemy.posicionActual.M43;

                    if (Vector3.Length(eye - vec1) <= Vector3.Length(eye - vec2))
                    {
                        enemy.posicionActual = enemy.posicionAnterior;
                        enemy.posicionActualHeadshot = enemy.posicionAnteriorHeadshot;
                        enemy.posicionActualChest = enemy.posicionAnteriorChest;
                        enemy.posicionActualLegs = enemy.posicionAnteriorLegs;
                        enemy.updateMovementMatrix(elapsedTime, new Vector3(0,0,0));
                    }
                    else
                    {
                        this.posicionActual = this.posicionAnterior;
                        this.posicionActualHeadshot = this.posicionAnteriorHeadshot;
                        this.posicionActualChest = this.posicionAnteriorChest;
                        this.posicionActualLegs = this.posicionAnteriorLegs;
                        this.updateMovementMatrix(elapsedTime, new Vector3(0,0,0));
                    }
                }
            }
            
            //Colision de enemigos con vegetacion, hecho para que no se queden trabados con o sin "ayuda" del player
            foreach (TgcMesh obstaculo in GameManager.Instance.Vegetation.Meshes)
            {
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    posicionActual = posicionAnterior;
                    posicionActualHeadshot = posicionAnteriorHeadshot;
                    posicionActualChest = posicionAnteriorChest;
                    posicionActualLegs = posicionAnteriorLegs;
                    vectorDireccionRotacion = vectorDireccionRotacionAnterior;
                    vectorDireccion = vectorDireccionAnterior;
                    Vector3 dirX = new Vector3(vectorDireccion.X, 0, 0);
                    dirX.Normalize();
                    updateMovementMatrix(elapsedTime, dirX);

                    TgcCollisionUtils.BoxBoxResult resultX = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                    if (resultX == TgcCollisionUtils.BoxBoxResult.Adentro || resultX == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        posicionActual = posicionAnterior;
                        posicionActualHeadshot = posicionAnteriorHeadshot;
                        posicionActualChest = posicionAnteriorChest;
                        posicionActualLegs = posicionAnteriorLegs;
                        Vector3 dirZ = new Vector3(0, 0, vectorDireccion.Z);
                        dirZ.Normalize();
                        updateMovementMatrix(elapsedTime, dirZ);

                        TgcCollisionUtils.BoxBoxResult resultZ = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                        if (resultZ == TgcCollisionUtils.BoxBoxResult.Adentro || resultZ == TgcCollisionUtils.BoxBoxResult.Atravesando)
                        {
                            posicionActual = posicionAnterior;
                            posicionActualHeadshot = posicionAnteriorHeadshot;
                            posicionActualChest = posicionAnteriorChest;
                            posicionActualLegs = posicionAnteriorLegs;
                            
                        }
                    }
                    break;
                }
            }

            posicionAnterior = posicionActual;
            posicionAnteriorHeadshot = posicionActualHeadshot;
            posicionAnteriorChest = posicionActualChest;
            posicionAnteriorLegs = posicionActualLegs;
            vectorDireccionRotacionAnterior = vectorDireccionRotacion;
            vectorDireccionAnterior = vectorDireccion;
            this.SonidoMovimiento.Position = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);
        }

        public Matrix calcularMatrizOrientacion(Vector3 v)
        {
            Matrix m_mWorld = new Matrix();
            Vector3 n = new Vector3(0, -1, 0);
            Vector3 w = Vector3.Cross(n, v);

            m_mWorld.M11 = v.X;
            m_mWorld.M12 = v.Y;
            m_mWorld.M13 = v.Z;
            m_mWorld.M14 = 0;

            m_mWorld.M21 = 0; 
            m_mWorld.M22 = 1;
            m_mWorld.M23 = 0;
            m_mWorld.M24 = 0;

            m_mWorld.M31 = w.X;
            m_mWorld.M32 = w.Y;
            m_mWorld.M33 = w.Z;
            m_mWorld.M34 = 0;

            m_mWorld.M41 = 0;
            m_mWorld.M42 = 0;
            m_mWorld.M43 = 0;
            m_mWorld.M44 = 1;

            return m_mWorld;
        }
        public Matrix CreatorMatrixPosition()
        {
            Random random = new Random();
            float ANGLE = random.Next(0, 360) / (int)Math.PI;

            Matrix fpsPos = Matrix.Translation(CustomFpsCamera.Instance.Position);

            Matrix radio = Matrix.Translation(this.SPAWN_RADIUS, SPAWN_HEIGHT, 0);

            Matrix escala = Matrix.Scaling(MESH_SCALE, MESH_SCALE, MESH_SCALE);

            Matrix giro = Matrix.RotationY(ANGLE);



            Matrix Resultado = escala * radio * giro * fpsPos;

            Normal = (CustomFpsCamera.Instance.Position - (new Vector3(Resultado.M41, 0, Resultado.M43)));
            // Normal = new Vector3(1, 0, 0);
            Normal.Normalize();
            return Resultado;


        }
        public override void Render(float elapsedTime)
        {

            setBaseEffectValues(elapsedTime);

            this.mesh.render();

            if (GameManager.Instance.drawBoundingBoxes)
            {
                this.mesh.BoundingBox.render();
                this.HEADSHOT_BOUNDINGBOX.render();
                this.CHEST_BOUNDINGBOX.render();
                this.LEGS_BOUNDINGBOX.render();
            }
            
        }

        virtual public void updateMovementMatrix(float elapsedTime, Vector3 Direccion)
        {
            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);
            
            float y;
            GameManager.Instance.interpoledHeight(vectorPosActual.X, vectorPosActual.Z, out y);
            float headOffsetY = posicionActualHeadshot.M42 - posicionActual.M42;
            float chestOffsetY = posicionActualChest.M42 - posicionActual.M42;
            float legsOffsetY = posicionActualLegs.M42 - posicionActual.M42;

            posicionActual.M42 = y;
            posicionActualHeadshot.M42 = headOffsetY + y;
            posicionActualChest.M42 = chestOffsetY + y;
            posicionActualLegs.M42 = legsOffsetY + y;

            MatOrientarObjeto = calcularMatrizOrientacion(vectorDireccionRotacion);

            Traslacion = Matrix.Translation(Direccion * MOVEMENT_SPEED * elapsedTime);

            Matrix transform = MatOrientarObjeto * posicionActual * Traslacion;
            this.mesh.Transform = transform;

            this.mesh.BoundingBox.transform(transform);

            posicionActual = posicionActual * Traslacion;
            this.HEADSHOT_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualHeadshot * Traslacion);
            posicionActualHeadshot = posicionActualHeadshot * Traslacion;

            this.CHEST_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualChest * Traslacion);
            posicionActualChest = posicionActualChest * Traslacion;

            this.LEGS_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualLegs * Traslacion);
            posicionActualLegs = posicionActualLegs * Traslacion;

           
        }
        
        public virtual void setBaseEffect()
        {
            mesh.Effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Shaders\\enemyBasic.fx");
            mesh.Technique = "HealthDependentShading";
        }

        public virtual void setBaseEffectValues(float elapsedTime)
        {
            mesh.Effect.SetValue("health", this.health);
            mesh.Effect.SetValue("g_time", elapsedTime);
        }

        override public void dispose(){
           
            this.SonidoMovimiento.play(false);
            this.SonidoMovimiento.stop();
           // this.SonidoMovimiento.dispose();
            this.mesh.dispose();
            this.CHEST_BOUNDINGBOX.dispose();
            this.HEADSHOT_BOUNDINGBOX.dispose();
            this.LEGS_BOUNDINGBOX.dispose();
        }

        virtual public void startAttack()
        {
            attacking = false;
        }

        virtual public void attack(float elapsedTime)
        {
            GameManager.Instance.player1.recibirAtaque(attackDamage, elapsedTime);
            GameManager.Instance.eliminarEnemigo(this);
        }
    }
}
