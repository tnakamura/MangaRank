import React from 'react'
import { graphql } from 'gatsby'
import { Row, Col } from 'reactstrap'
import TagLink from '../components/atoms/tag-link'
import Pagination from '../components/molecules/pagination'
import Layout from '../components/organisms/layout'

export default class TagsTemplate extends React.Component {
  render() {
    const tags = this.props.data.allTagsJson.edges
    const { page, numPages } = this.props.pageContext
    return (
      <Layout title="タグ">
        <Row>
          {tags.map(({ node }) => <Col md={4} sm={12}>
            <TagLink name={node.name}/>
          </Col>)}
        </Row>

        <Row>
          <Col md={12}>
            <Pagination className="justify-content-center mt-5"
                        basePath="/tags"
                        page={page}
                        numPages={numPages} />
          </Col>
        </Row>
      </Layout>
    )
  }
}

export const tagsQuery = graphql`
  query tagsQuery($skip: Int!, $limit: Int!) {
    allTagsJson(
      sort: { fields: [count], order: DESC }
      limit: $limit
      skip: $skip
    ) {
      edges {
        node {
          name
          count
        }
      }
    }
  }
`
