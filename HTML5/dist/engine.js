(function(root){
'use strict';
const D=root.REPORT_DATA;
const themes=[
 ['#6d8170','#465c4f','#293f37','#e9b578','Tichý začiatok medzi hviezdami.','ground'],
 ['#7d9a66','#526f46','#2e503b','#c7ef82','Lesy, ktoré čakajú na nový život.','water'],
 ['#739595','#426b73','#264d58','#92e1e8','Zabudnutý svet severských strážcov.','swamp'],
 ['#a38ab0','#716483','#493d5f','#e1b2f5','Dve polovice jedného tajomstva.','water'],
 ['#96928a','#686b68','#3e4e4c','#ffcb86','Oceľové srdce galaxie.','ground'],
 ['#c4a27b','#967958','#625342','#f9dd9c','Piesok, slnko a staré príbehy.','swamp'],
 ['#7aaca6','#487c7d','#2b5c66','#a1f3de','Súostrovie nekonečných ciest.','water'],
 ['#a57c79','#775456','#503c48','#ffa28a','Krajina ohňa a dračích legiend.','lava'],
 ['#c0ad78','#9b875c','#6a6042','#ffe69d','Tajomstvá pod zlatým pieskom.','ground'],
 ['#9aab8a','#67826b','#3f5e54','#e3b0d0','Posledná iskra na konci výpravy.','water']
];
const labels={Wood:'Drevo',Rock:'Kameň',Plank:'Dosky',Steel:'Oceľ',Coin:'Mince',Azurite:'Azurit',Amethyst:'Ametyst',Diamond:'Diamant',Ruby:'Rubín',Coal:'Uhlie',Sulphur:'Síra',Uranium:'Urán',Gear:'Ozubené koleso',Token:'Žetóny',axe:'Sekera',pickaxe:'Krompáč',sword:'Meč',armor:'Brnenie'};
const name=k=>labels[k]||k;
function fresh(){return {version:1,planet:0,unlocked:0,inventory:{Wood:0,Rock:0,Plank:0,Steel:0,Coin:0},tools:{axe:0,pickaxe:0,sword:0,armor:0},restored:[],gathered:0,kills:0,crafted:0,xp:0,prestige:0,home:0,workers:0,workerTime:Date.now(),jobs:[],refineryLevel:0,hp:100,position:{x:0,y:2},visited:[0],sound:false,started:false,challenge:0};}
function normalize(raw){const s=fresh();if(!raw||raw.version!==1)return s;const num=(v,max=1e9)=>Number.isFinite(v)&&v>=0?Math.min(v,max):0;for(const k of ['unlocked','planet'])s[k]=Math.floor(num(raw[k],9));s.planet=Math.min(s.planet,s.unlocked);for(const k of ['gathered','kills','crafted','xp','prestige','home','workers','refineryLevel','challenge'])s[k]=Math.floor(num(raw[k],k==='home'||k==='workers'?5:k==='refineryLevel'?20:1e9));for(const k of Object.keys(s.tools))s.tools[k]=Math.floor(num(raw.tools?.[k],73));for(const [k,v]of Object.entries(raw.inventory||{}))if(/^[A-Za-z0-9_]+$/.test(k)&&k!=='__proto__'&&k!=='constructor')s.inventory[k]=num(v);s.restored=[...new Set((Array.isArray(raw.restored)?raw.restored:[]).filter(x=>Number.isInteger(x)&&x>=0&&x<=s.unlocked))];s.visited=[...new Set([0,...(Array.isArray(raw.visited)?raw.visited:[]).filter(x=>Number.isInteger(x)&&x>=0&&x<=s.unlocked)])];s.hp=Math.max(1,num(raw.hp,100+s.home*20));if(Number.isFinite(raw.position?.x)&&Number.isFinite(raw.position?.y)&&Math.hypot(raw.position.x,raw.position.y)<16)s.position=raw.position;s.sound=!!raw.sound;s.started=!!raw.started;s.workerTime=Math.min(Date.now(),num(raw.workerTime)||Date.now());s.jobs=(Array.isArray(raw.jobs)?raw.jobs:[]).filter(j=>['Plank','Steel'].includes(j.product)&&Number.isFinite(j.end)).slice(0,8).map(j=>({product:j.product,end:Math.min(Date.now()+120000,j.end),amount:j.product==='Plank'?10:8}));return s;}
function afford(s,cost){return Object.entries(cost).every(([k,v])=>(s.inventory[k]||0)>=v);}
function pay(s,cost){if(!afford(s,cost))return false;for(const [k,v]of Object.entries(cost))s.inventory[k]-=v;return true;}
function grant(s,k,n){s.inventory[k]=(s.inventory[k]||0)+n;}
function upgradeCost(s,k){const l=s.tools[k];return {Wood:12+l*7,Rock:8+l*5,Coin:15+l*12,...(l>=7?{Steel:2+Math.floor(l/7)}:{})};}
function upgrade(s,k){if(!Object.hasOwn(s.tools,k))return false;const cap=k==='armor'?D.planets[s.unlocked].chestplate:D.planets[s.unlocked][k];if(s.tools[k]>=cap||!pay(s,upgradeCost(s,k)))return false;s.tools[k]++;return true;}
function restoreCost(s){let i=s.planet;return i===0?{Wood:20}: {Wood:30+i*10,Rock:20+i*8,...(i>=2?{Plank:10+Math.floor(i/3)*5}:{}),...(i>=4?{Steel:8+Math.floor(i/3)*4}:{})};}
function restore(s){if(s.restored.includes(s.planet)||!pay(s,restoreCost(s)))return false;s.restored.push(s.planet);s.unlocked=Math.min(9,Math.max(s.unlocked,s.planet+1));s.xp+=30+s.planet*20;grant(s,'Coin',60+s.planet*40);return true;}
function recipe(product){const r=D.recipes.find(r=>r.name==='RefineryDataHomeResource'+product);const t=r.tiers[0];return {cost:Object.fromEntries(t.inputs.map(x=>[x.resource,x.amount])),amount:t.outputs[0].amount,duration:t.duration};}
function craft(s,product,now=Date.now()){if(!['Plank','Steel'].includes(product)||s.jobs.filter(j=>j.product===product).length>=3)return false;const r=recipe(product);if(!pay(s,r.cost))return false;const last=Math.max(now,...s.jobs.filter(j=>j.product===product).map(j=>j.end));s.jobs.push({product,amount:r.amount,end:last+r.duration*1000*Math.pow(.9825,s.refineryLevel)});return true;}
function production(s,now=Date.now()){const done=s.jobs.filter(j=>j.end<=now);for(const j of done){grant(s,j.product,j.amount);s.crafted+=j.amount;s.xp+=5;}s.jobs=s.jobs.filter(j=>j.end>now);const ticks=Math.floor(Math.max(0,Math.min(now-s.workerTime,3600000))/5000);if(ticks){if(s.workers){grant(s,'Wood',ticks*s.workers);grant(s,'Rock',ticks*s.workers);}s.workerTime=now;}return done;}
function travel(s,i){if(!Number.isInteger(i)||i<0||i>s.unlocked)return false;s.planet=i;s.position={x:0,y:2};s.hp=100+s.home*20;if(!s.visited.includes(i))s.visited.push(i);return true;}
function power(s,type){const rec=D.tools[type]?.find(t=>t.level===s.tools[type]);return rec?.attributes?.[0]?.min||1;}
function terrain(x,y,p){const t=themes[p][5];return t!=='ground'&&Math.hypot(x+5,y+1)<3.3?t:'ground';}
function rng(seed){return()=>{seed=(Math.imul(seed,1664525)+1013904223)|0;return(seed>>>0)/4294967296;};}
function world(p){let rand=rng(349+p*179),nodes=[];let extras=D.planets[p].resources.map(id=>D.resourceNames[id]).filter(x=>x&& !['Wood','Rock','Coin','Plank','Steel'].includes(x));extras=extras.slice(0,6);for(let i=0;i<66;i++){let a=rand()*Math.PI*2,r=3+rand()*11.4,x=Math.cos(a)*r,y=Math.sin(a)*r;if(Math.hypot(x-1,y+2)<3.2||Math.hypot(x+5,y+1)<3.4)continue;let type=i%4===0?'Rock':i%6===0&&p>0?(extras[i%extras.length]||'Azurite'):'Wood';nodes.push({id:i,x,y,type,hp:3+p,maxHp:3+p,respawn:0,shake:0,scale:.8+rand()*.5});}nodes.unshift({id:99,x:1.8,y:3.4,type:'Wood',hp:3+p,maxHp:3+p,respawn:0,shake:0,scale:1});const enemies=[];if(p>0){for(let i=0;i<4+p;i++){let a=i*2.4;enemies.push({x:Math.cos(a)*11,y:Math.sin(a)*11,hp:14+p*6,maxHp:14+p*6,hit:0,respawn:0,boss:false});}enemies.push({x:0,y:-11,hp:50+p*20,maxHp:50+p*20,hit:0,respawn:0,boss:true});}return {nodes,enemies,seed:p};}
const API={D,themes,name,fresh,normalize,afford,pay,grant,upgradeCost,upgrade,restoreCost,restore,recipe,craft,production,travel,power,terrain,rng,world};root.GameCore=API;if(typeof module!=='undefined')module.exports=API;
})(typeof window!=='undefined'?window:globalThis);
