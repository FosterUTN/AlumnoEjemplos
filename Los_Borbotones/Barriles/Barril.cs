﻿using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Barril : GameObject


    {
        public State estado = new Activo();

        
        float EXPLOSION_RADIUS = 300f;
        public TgcBoundingSphere explosion;

        public override void Init()
        {
            explosion = new TgcBoundingSphere(mesh.Position,EXPLOSION_RADIUS);
            
        }

        public override void Update(float elapsedTime)
        {
            estado.Update(elapsedTime, this);
        }

       
         public override void Render(float elapsedTime)
         {

             estado.Render(elapsedTime, this);
                 
         }

         public void explotar()
         {
             estado.explotar(this);
         }
    }
}
