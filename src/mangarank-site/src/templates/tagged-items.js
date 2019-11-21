import React from 'react'
import { graphql } from 'gatsby'
import { Row, Col } from 'reactstrap'
import Layout from '../components/organisms/layout'
import ItemList from '../components/organisms/item-list'
import Pagination from '../components/molecules/pagination'

export default class TaggedItemsTemplate extends React.Component {
  getItems() {
    return this.props.data.allItemsJson.edges.map(e => e.node)
  }

  getTag() {
    return this.props.pageContext.tag
  }

  render() {
    const items = this.getItems()
    const tag = this.getTag()
    const {
      page,
      numPages,
      limit,
    } = this.props.pageContext
    return (
      <Layout title={`'${tag}' タグがついたマンガ`}>
        <Row>
          <Col md={12}>
            <h1>'{tag}' タグがついたマンガ</h1>
          </Col>
        </Row>

        <ItemList items={items}
                  page={page}
                  perPage={limit}/>

        <Row>
          <Col md={12}>
            <Pagination className="justify-content-center mt-5"
                        basePath={`/items/tagged/${tag}`}
                        page={page}
                        numPages={numPages} />
          </Col>
        </Row>
      </Layout>
    )
  }
}

export const taggedItemsQuery = graphql`
  query taggedItemsQuery($tag:String!, $skip: Int!, $limit: Int!) {
    allItemsJson(
      limit: $limit
      skip: $skip
      sort: { fields: [score], order: DESC }
      filter: {
        tags: {
          elemMatch:{
            name:{
              eq: $tag
            }
          }
        }
      }
    ) {
      edges {
        node {
          asin
          title
          description
          detailPageUrl
          author
          publisher
          imageUrl
          score
          tags {
            name
          }
        }
      }
    }
  }
`
