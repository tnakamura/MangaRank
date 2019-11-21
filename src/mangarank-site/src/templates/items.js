import React from 'react'
import { graphql } from 'gatsby'
import { Row, Col } from 'reactstrap'
import Layout from '../components/organisms/layout'
import ItemList from '../components/organisms/item-list'
import Pagination from '../components/molecules/pagination'

export default class ItemsTemplate extends React.Component {
  getItems() {
    return this.props.data.allItemsJson.edges.map(e => e.node)
  }

  render() {
    const items = this.getItems()
    const {
      page,
      numPages,
      limit,
    } = this.props.pageContext

    return (
      <Layout title="マンガ">
        <ItemList items={items}
                  page={page}
                  perPage={limit}/>

        <Row>
          <Col md={12}>
            <Pagination className="justify-content-center mt-5"
                        basePath="/items"
                        page={page}
                        numPages={numPages}/>
          </Col>
        </Row>
      </Layout>
    )
  }
}

export const itemsQuery = graphql`
  query itemsQuery($skip: Int!, $limit: Int!) {
    allItemsJson(
      sort: { fields: [score], order: DESC }
      limit: $limit
      skip: $skip
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
