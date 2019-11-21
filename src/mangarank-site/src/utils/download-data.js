const https = require('https');
const fs = require('fs');
const path = require('path');

function download(url, outPath) {
  const outFile = fs.createWriteStream(outPath, 'utf8');
  https.get(url, (res) => {
    res
      .pipe(outFile)
      .on('end', () => {
        outFile.close();
      })
      .on('error', (err) => {
        console.error(err);
      });
  });
}

const baseUrl = process.env.CLOUD_STORAGE_BASE_URL;
const itemsUrl = baseUrl + '/items.json';
const tagsUrl = baseUrl + '/tags.json';
const entriesUrl = baseUrl + '/entries.json';

const outDir = path.join(__dirname, '..', 'data');
const outItemsPath = path.join(outDir, 'items.json');
const outTagsPath = path.join(outDir, 'tags.json');
const outEntriesPath = path.join(outDir, 'entries.json');

download(itemsUrl, outItemsPath);
download(tagsUrl, outTagsPath);
download(entriesUrl, outEntriesPath);
