import React from 'react'
import { graphql } from 'gatsby'
import Img from 'gatsby-image'
import { Row, Col } from 'reactstrap'

import Layout from '../components/organisms/layout'
import TagList from '../components/organisms/tag-list'
import EntryList from '../components/organisms/entry-list'
import AmazonButton from '../components/molecules/amazon-button'

export default class ItemTemplate extends React.Component {
  getEntries() {
    if (this.props.data.allEntriesJson) {
      return this.props.data.allEntriesJson.edges
    } else {
      return []
    }
  }

  render() {
    const item = this.props.data.itemsJson
    const entries = this.getEntries()
    return (
      <Layout title={item.title}>
        <Row>
          <Col md={2} sm={12}>
            <Row>
              <Col md={12}>
                <Img fixed={{
                       width: 110,
                       height: 160,
                       src: item.imageUrl,
                       srcSet: item.imageUrl
                     }}
                     alt={item.title}/>
              </Col>
            </Row>
          </Col>

          <Col md={7} sm={12}>
            <Row>
              <Col md={12}>
                <h1>{item.title}</h1>
              </Col>

              <Col className="mt-4" md={12}>
                <p dangerouslySetInnerHTML={{__html: item.description}}/>
              </Col>

              <Col className="mt-4" md={12}>
                <h2 className="mb-3">本書が紹介されている記事</h2>
                <EntryList entries={entries} />
              </Col>
            </Row>
          </Col>

          <Col md={3} sm={12}>
            <Row>
              <Col md={12}>
                <AmazonButton detailPageUrl={item.detailPageUrl} block/>
              </Col>
              <Col className="mt-4" md={12}>
                <h2 className="mb-3">著者</h2>
                <p>{item.author}</p>
              </Col>
              <Col className="mt-4" md={12}>
                <h2 className="mb-3">出版社</h2>
                <p>{item.publisher}</p>
              </Col>
              <Col className="mt-4" md={12}>
                <h2 className="mb-3">タグ</h2>
                <TagList tags={item.tags} inline />
              </Col>
            </Row>
          </Col>
        </Row>
      </Layout>
    )
  }
}

export const itemQuery = graphql`
  query itemQuery($asin: String!) {
    itemsJson(
      asin: { eq: $asin }
    ) {
      asin
      title
      imageUrl
      detailPageUrl
      description
      author
      publisher
      score
      tags {
        name
      }
    }
    allEntriesJson(
      filter: { asin: { eq: $asin } }
    ) {
      edges {
        node {
          id
          title
          url
        }
      }
    }
  }
`
