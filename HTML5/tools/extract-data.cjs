/* Offline, read-only extraction of the supplied REA report. No packages required. */
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const source = process.argv[2] || 'C:/Users/bumbe/Downloads/rea-savetheworld-report';
const output = path.resolve(__dirname, '../dist/data.js');
const read = name => JSON.parse(fs.readFileSync(path.join(source, name), 'utf8'));
const records = read('decoded-scriptables.json');
const settings = read('game-settings.json');
for (const [k,v] of Object.entries(settings)) {
  if (typeof v === 'string' && /^[\[{]/.test(v.trim())) { try { settings[k] = JSON.parse(v); } catch {} }
}
const group = x => x.file.split('_')[0];
const data = prefix => records.filter(x=>group(x)===prefix).map(x=>x.data);
const clean = value => JSON.parse(JSON.stringify(value, (key,v) => typeof v === 'number' && !Number.isInteger(v) ? Number(v.toFixed(6)) : v));
const attrs = d => (d.attributeModifiers || []).map(a=>({type:a.type,valueType:a.valueType,min:a.valueRange.min,max:a.valueRange.max}));
const resources = data('resourcedatascriptables').filter(d=>d.key).map(d=>({name:d.m_Name,key:d.key,type:d.type,title:d.title,toolType:d.toolType,requiredToolLevel:d.requiredToolLevel,health:d.health,regenerationRate:d.regenerationRate,resourcesPerDamage:d.resourcesPerDamage}));
// The supplied enum-values.json reads byte enums as int32. Use observed records instead.
const resourceNames = {};
for (const r of resources) if (!resourceNames[r.type]) resourceNames[r.type] = r.title || r.key;
const tools = {};
for (const kind of ['axe','pickaxe','sword']) tools[kind] = data(kind+'scriptables').filter(d=>d.isPlayerTool&&!d.isSkin&&!d.isGolden).map(d=>({name:d.m_Name,level:d.level,type:d.type,attributes:attrs(d)})).sort((a,b)=>a.level-b.level);
const planets = data('levelplanetdata').filter(d=>/^LevelPlanetDataLevel\d+$/.test(d.m_Name)).map(d=>({name:d.m_Name,title:d.title,index:d.levelIndex,radius:d.radius,gravity:d.gravity,resources:d.resources,axe:d.axeMaxUpgradeLevel,pickaxe:d.pickaxeMaxUpgradeLevel,sword:d.swordMaxUpgradeLevel,chestplate:d.chestplateMaxUpgradeLevel,helmet:d.helmetMaxUpgradeLevel})).sort((a,b)=>a.index-b.index);
const product = p=>({key:p.productKey,resource:p.productKey.replace(/^ProductDataResource/,''),amount:p.amount});
const recipes = records.map(x=>x.data).filter(d=>d.productTiers&&d.requirementTiers).map(d=>({name:d.m_Name,key:d.key,type:d.refineryType,toolType:d.toolType,duration:d.refiningDuration,queue:d.refiningQueue,upgradable:!!d.upgradable,tiers:d.productTiers.map((t,i)=>({duration:d.requirementTiers[i]?.refineDuration || d.refiningDuration,inputs:(d.requirementTiers[i]?.resourceCollections || []).flatMap(c=>c.resources||[]).map(r=>({resourceType:r.resourceType,resource:resourceNames[r.resourceType] || 'Resource '+r.resourceType,amount:r.amount})),outputs:(t.products||[]).map(product)}))}));
const enemies = data('pawnsdata').filter(d=>/Enemy/.test(d.m_Name)&&d.baseAttributes).map(d=>{
  const attr = k=>d.baseAttributes['<'+k+'>k__BackingField']?.valueRange;
  return {name:d.m_Name,title:d.pawnName,boss:!!d.isBoss,health:attr('Health'),damage:attr('Damage'),attackSpeed:attr('AttackSpeed'),aggroRange:attr('AggroRange'),loot:(d.lootDrops?.value||[]).map(l=>(l.products||[]).map(product))};
});
// Compact full records remove Unity link wrappers, retaining their IDs as strings.
function compact(v) {
  if (Array.isArray(v)) return v.map(compact);
  if (!v || typeof v!=='object') return typeof v==='number'&&!Number.isInteger(v)?Number(v.toFixed(6)):v;
  if ('m_PathID' in v && 'm_FileID' in v) return v.m_PathID==='0'?null:'@'+v.m_FileID+':'+v.m_PathID;
  if ('m_AssetGUID' in v) return v.m_AssetGUID ? 'guid:'+v.m_AssetGUID : null;
  return Object.fromEntries(Object.entries(v).filter(([k])=>!['m_GameObject','m_Enabled','m_Script'].includes(k)).map(([k,x])=>[k,compact(x)]));
}
const counts = {};
for(const r of records) counts[group(r)] = (counts[group(r)]||0)+1;
const report = clean({
  provenance:{package:'com.savetheworld.game',version:'2.9.1',decodedObjects:records.length,sourceSha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(source,'decoded-scriptables.json'))).digest('hex'),enumWarning:'Supplied integer enum values are invalid; resource names are derived from actual typed ResourceData records.'},
  counts,planets,tools,resources,resourceNames,recipes,enemies,
  armor:data('armorscriptables').map(d=>({name:d.m_Name,type:d.type,level:d.level,attributes:attrs(d)})),
  movements:data('movementprofiles').filter(d=>/^MovementProfilePlayer/.test(d.m_Name)).map(d=>({name:d.m_Name,acceleration:d.acceleration,maxVelocity:d.maxVelocity,damping:d.damping})),
  prestige:data('prestigedata')[0]?.prestigeRewards || [],
  refineryUpgrades:data('refineryupgradedata').map(compact),
  quests:{sequence:settings.homeQuestSequence?.split(';')||[],home:settings.homeQuests || {},delivery:settings.availableDeliveryQuests,hunt:settings.availableHuntQuests},
  settings,
  catalog:records.map((r,i)=>({id:i,name:r.data.m_Name || r.data.key || 'Object '+r.pathId,category:group(r),pathId:r.pathId,data:compact(r.data)}))
});
fs.mkdirSync(path.dirname(output),{recursive:true});
fs.writeFileSync(output,'/* Generated from supplied static report; see DATA_SOURCES.md. */\nwindow.REPORT_DATA='+JSON.stringify(report)+';\n');
console.log(JSON.stringify({output,bytes:fs.statSync(output).size,planets:report.planets.length,tools:Object.fromEntries(Object.entries(tools).map(([k,v])=>[k,v.length])),resources:resources.length,recipes:recipes.length,enemies:enemies.length,catalog:report.catalog.length}));
