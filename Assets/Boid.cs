using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid;
    private Neighborhood neighborhood;
    void Awake(){
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        // Выбрать случайную начальную позицию
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        // Выбрать случайную начальную скорость
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;

        LookAhead();

        // Окрасить птицу в случайный цвет
        Color randColor = Color.black;
        while(randColor.r + randColor.g + randColor.b < 1.0f){
            randColor = new Color(Random.value, Random.value , Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer r in rends){
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }
    void LookAhead(){
        // Ориентировать птицу клювом в сторону полета
        transform.LookAt(pos + rigid.velocity);
    }
    public Vector3 pos 
    {
        get {return transform.position;}
        set {transform.position = value ;}
    }
    void FixedUpdate(){
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;
        // ПРЕДОТВРАЩЕНИЕ СТОЛКНОВЕНИЙ - избегать близких соседей
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        // Если получени вектор Vector.zero , ничего предпринимать не надо
        if(tooClosePos != Vector3.zero){
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }
        // СОГЛАСОВАНИЕ СКОРОСТИ - попробовать согласовать скорость с соседями
        Vector3 velAlign = neighborhood.avgVel;
        // Согласование требуется, только если velAlign не равно Vector3.zero
        if(velAlign != Vector3.zero){
            // нормальзуем скорость
            velAlign.Normalize();
            // преобразую в выбранную скорость 
            velAlign *= spn.velocity;
        }
        // КОНЦЕТРАЦИЯ СОСЕДЕЙ - движение в сторону центра группы соседей
        Vector3 velCenter = neighborhood.avgPos;
        if(velCenter != Vector3.zero){
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        // ПРИТЯЖЕНИЕ - организация движения в сторону объекта Attractor
        Vector3 delta = Attaractor.POS - pos ;
        // Проверить куда двигаться в сторону Attracor или от него
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 vellAttract = delta.normalized * spn.velocity;

        // Применить все скорости 
        float fdt = Time.fixedDeltaTime;
        if(velAvoid != Vector3.zero){
            vel = Vector3.Lerp(vel , velAvoid, spn.collAvoid*fdt);
        }
        else{
            if(velAlign != Vector3.zero){
            vel = Vector3.Lerp(vel, velAlign,spn.velMatching*fdt);
            } 
            if(velCenter != Vector3.zero){
            vel = Vector3.Lerp(vel, velCenter,spn.flockCentering*fdt);
            }
        if(vellAttract != Vector3.zero){
        if(attracted){
            vel = Vector3.Lerp(vel , vellAttract, spn.attractPull*fdt);
        }
        else {
            vel = Vector3.Lerp(vel, -vellAttract, spn.attractPush*fdt);
        }
        }
        }
        // Установить vel в соответствии с velocity в объекте-одиночке Spawner
        vel = vel.normalized * spn.velocity;
        // в заключении присвоить скорость компоненту Rigidbody
        rigid.velocity = vel;
        // провернуть птицу клювом в сторону нового направления движения
        LookAhead();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
