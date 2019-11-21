/**
 * Implement Gatsby's Node APIs in this file.
 *
 * See: https://www.gatsbyjs.org/docs/node-apis/
 */

// You can delete this file if you're not using it

const path = require("path")
const { createFilePath } = require("gatsby-source-filesystem")

exports.createPages = ({ graphql, actions }) => {
  const { createPage } = actions

  return new Promise((resolve, reject) => {
    resolve(
      createItemsPages(
        graphql,
        createPage,
        reject
      ).then(() => {
        return createTagsPages(
          graphql,
          createPage,
          reject
        ).then(tags => {
          const promises = tags.map(edge => {
            return createTaggedItemsPages(
              graphql,
              createPage,
              reject,
              edge.node
            )
          })
          return promises.reduce((prev, current) => prev.then(current))
        })
      })
    )
  })
}

function createItemsPages(graphql, createPage, reject) {
  return graphql(`
    {
      allItemsJson(
        sort: { fields: [score], order: DESC }
        limit: 1000
      ) {
        edges {
          node {
            asin
          }
        }
      }
    }
  `).then(result => {
    if (result.errors) {
      reject(result.errors)
    }

    // ページ分割したコミック一覧を作成
    const items = result.data.allItemsJson.edges
    const itemsPerPage = 20
    const numPages = Math.ceil(items.length / itemsPerPage)
    Array.from({ length: numPages }).forEach((_, i) => {
      const context = {
        limit: itemsPerPage,
        skip: i * itemsPerPage,
        numPages: numPages,
        page: i + 1,
      }
      if (i === 0) {
        createPage({
          path: `/`,
          component: path.resolve("./src/templates/items.js"),
          context,
        })
      }
      createPage({
        path: i === 0 ? `/items` : `/items/${i + 1}`,
        component: path.resolve("./src/templates/items.js"),
        context,
      })
    })

    // 詳細ページを作成
    items.forEach(({ node }) => {
      createPage({
        path: `/items/${node.asin}`,
        component: path.resolve("./src/templates/item.js"),
        context: {
          asin: node.asin,
        }
      })
    });

    return Promise.resolve(items)
  })
}

function createTagsPages(graphql, createPage, reject) {
  return graphql(`
    {
      allTagsJson(
        sort: { fields: [count], order: DESC }
        limit: 1000
      ) {
        edges {
          node {
            name
            count
          }
        }
      }
    }
  `).then(result => {
    if (result.errors) {
      reject(result.errors)
    }

    // ページ分割したタグ一覧を作成
    const tags = result.data.allTagsJson.edges
    const tagsPerPage = 60
    const numPages = Math.ceil(tags.length / tagsPerPage)
    Array.from({ length: numPages }).forEach((_, i) => {
      createPage({
        path: i === 0 ? `/tags` : `/tags/${i + 1}`,
        component: path.resolve("./src/templates/tags.js"),
        context: {
          limit: tagsPerPage,
          skip: i * tagsPerPage,
          numPages: numPages,
          page: i + 1,
        }
      })
    })

    return Promise.resolve(tags)
  })
}

function createTaggedItemsPages(graphql, createPage, reject, tag) {
  return graphql(`
    {
      allItemsJson(
        limit: 1000
        sort: { fields: [score], order: DESC }
        filter: {
          tags: {
            elemMatch:{
              name:{
                eq:"${tag.name}"
              }
            }
          }
        }
      ) {
        edges {
          node {
            asin
          }
        }
      }
    }
  `).then(result => {
    if (result.errors) {
      reject(result.errors)
    }
    
    // タグが付いたコミックが1件もないときは allItemsJson
    // は null なのでスキップ
    if (!result.data.allItemsJson) {
      return Promise.resolve([])
    }

    // ページ分割したコミック一覧を作成
    const items = result.data.allItemsJson.edges
    const itemsPerPage = 20
    const numPages = Math.ceil(items.length / itemsPerPage)
    Array.from({ length: numPages }).forEach((_, i) => {
      createPage({
        path: i === 0 ? `/items/tagged/${tag.name}` : `/items/tagged/${tag.name}/${i + 1}`,
        component: path.resolve("./src/templates/tagged-items.js"),
        context: {
          limit: itemsPerPage,
          skip: i * itemsPerPage,
          tag: tag.name,
          numPages: numPages,
          page: i + 1,
        }
      })
    })

    return Promise.resolve(items)
  })
}
