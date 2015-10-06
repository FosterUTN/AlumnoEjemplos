﻿using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Sound;


namespace AlumnoEjemplos.Los_Borbotones 
{

    class Enemy_lvl_1:Enemy
    {

        TgcSkeletalMesh skeletalMesh;
        public event TgcViewer.Utils.TgcSkeletalAnimation.TgcSkeletalMesh.AnimationEndsHandler AnimationEnd;
        override
            public void Init(){
                health = 100;
                score = 1;
             Device d3dDevice = GuiController.Instance.D3dDevice;
             MESH_SCALE = 0.5f;
             
             attackDamage = 25;
             TgcSceneLoader loader = new TgcSceneLoader();
             TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
             this.mesh = scene.Meshes[0];
             giroInicial = Matrix.RotationY(-(float)Math.PI / 2);

            
            //carga de animaciones
             TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
             skeletalMesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                 GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                 new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                   GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Patear-TgcSkeletalAnim.xml",
                });
             skeletalMesh.playAnimation("Caminando", true);
             skeletalMesh.AnimationEnds += this.onAnimationEnds;
             base.Init();
             HEADSHOT_BOUNDINGBOX = this.mesh.BoundingBox.clone();
             CHEST_BOUNDINGBOX = this.mesh.BoundingBox.clone();
             LEGS_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            Matrix escalabox = Matrix.Scaling(new Vector3(0.43f,0.3f,0.43f));
            Matrix traslationbox = Matrix.Translation(new Vector3(0,90f,0));
            HEADSHOT_BOUNDINGBOX.transform(escalabox * traslationbox);
            posicionActualHeadshot = escalabox * traslationbox * posicionActual;
           Matrix escalabox2 = Matrix.Scaling(new Vector3(0.6f, 0.3f, 0.6f));
            Matrix traslationbox2 = Matrix.Translation(new Vector3(0, 50f, 0));
            CHEST_BOUNDINGBOX.transform(escalabox2 * traslationbox2);
            posicionActualChest = escalabox2 * traslationbox2 * posicionActual;
            Matrix escalabox3 = Matrix.Scaling(new Vector3(0.4f, 0.38f, 0.4f));
            Matrix traslationbox3 = Matrix.Translation(new Vector3(0, 0f, 0));
            LEGS_BOUNDINGBOX.transform(escalabox3 * traslationbox3);
            posicionActualLegs = escalabox3 * traslationbox3 * posicionActual;
            skeletalMesh.AutoTransformEnable = false;

            //carga de sonido
            SonidoMovimiento = new Tgc3dSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Audio\\Robot\\servomotor.wav", new Vector3(posicionActual.M41,posicionActual.M42,posicionActual.M43));
            SonidoMovimiento.MinDistance = 100f;
            SonidoMovimiento.play(true);
            
            
            setBaseEffect();

        }
        override
        public void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
            this.skeletalMesh.Transform = MatOrientarObjeto * posicionActual * Traslacion;
        }
        public override void Render(float elapsedTime)
        {
            setBaseEffectValues(elapsedTime);

            skeletalMesh.animateAndRender();
            if (GameManager.Instance.drawBoundingBoxes)
            {
                this.mesh.BoundingBox.render();
                this.HEADSHOT_BOUNDINGBOX.render();
                this.CHEST_BOUNDINGBOX.render();
                this.LEGS_BOUNDINGBOX.render();
            }
        }

        /*
        public override void setBaseEffect()
        {
            skeletalMesh.Effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Shaders\\enemyBasic.fx");
            skeletalMesh.Technique = "HealthDependentShading";
        }

        public override void setBaseEffectValues(float elapsedTime)
        {
            skeletalMesh.Effect.SetValue("health", this.health);
            skeletalMesh.Effect.SetValue("g_time", elapsedTime);
        }
        */
        public override void dispose()
        {
            base.dispose();
            skeletalMesh.dispose();
        }

        public override void attack(float elapsedTime)
        {
            if (attacking && !attacked) 
            {
                GameManager.Instance.player1.recibirAtaque(attackDamage, elapsedTime);
                attacked = true;
            }
        }

        public override void startAttack()
        {
            MOVEMENT_SPEED *= 3;
            skeletalMesh.playAnimation("Patear", false);
            attackDelay = 2;
            attacking = true;
        }

        protected virtual void onAnimationEnds(TgcSkeletalMesh mesh)
        {
            if (attacking)
            {
                MOVEMENT_SPEED = MOVEMENT_SPEED / 3;
                skeletalMesh.playAnimation("Caminando");
                attacking = false;
                attacked = false;
            }
        }
    }
}
