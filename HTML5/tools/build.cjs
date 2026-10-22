const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const root=path.resolve(__dirname,'..'),dist=path.join(root,'dist');
for(const file of ['data.js','engine.js','game.js'])new vm.Script(fs.readFileSync(path.join(dist,file),'utf8'),{filename:file});
let html=fs.readFileSync(path.join(dist,'index.html'),'utf8');
html=html.replace('<link rel="stylesheet" href="style.css">',()=>'<style>'+fs.readFileSync(path.join(dist,'style.css'),'utf8')+'</style>');
for(const file of ['data.js','engine.js','game.js'])html=html.replace(`<script src="${file}"></script>`,()=>'<script>'+fs.readFileSync(path.join(dist,file),'utf8').replace(/<\/script/gi,'<\\/script')+'</script>');
fs.writeFileSync(path.join(root,'Save-the-World.html'),html);
console.log('Built standalone HTML: '+(Buffer.byteLength(html)/1024/1024).toFixed(2)+' MB');
