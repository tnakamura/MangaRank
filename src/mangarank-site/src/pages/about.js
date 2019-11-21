import React from 'react'
import { graphql, Link } from 'gatsby'
import { Row, Col } from 'reactstrap'
import Layout from '../components/organisms/layout'

const AboutPage = ({ data }) => (
  <Layout title="サイトについて">
    <Row>
      <Col md={12}>
        <h4 className="mb-3">
          このサイトについて
        </h4>
        <p>
          {data.site.siteMetadata.title} は面白そうなマンガに出会うためのマンガランキングサイトです。
          はてなブログの中からマンガに関する記事を集計して作成しました。
        </p>
      </Col>
      <Col md={12}>
        <h4 className="mt-4 mb-3">
          運営者
        </h4>
        <p>
          当サイトは、tnakamura が個人で運営しております。 
          ご連絡の際は、<Link to="/contact">こちらからご連絡ください。</Link>
        </p>
      </Col>

      <Col md={12}>
        <h4 className="mt-4 mb-3">
          免責事項
        </h4>
        <p>
          使用している版権物の知的所有権は、それぞれの著作者・団体に帰属しております。
          著作権所有者様からの警告及び修正、撤去のご連絡があった場合は、
          迅速に対処または削除いたします。また、掲載内容に関しては、
          万全を期しておりますが、その内容を保証するものではありません。
        </p>
      </Col>

      <Col md={12}>
        <h4 className="mt-4 mb-3">
          Amazon.co.jpアソシエイトに関するプライバシーポリシー
        </h4>
        <p>
          当サイト {data.site.siteMetadata.title} は、amazon.co.jp を宣伝しリンクすることによって
          サイトが紹介料を獲得できる手段を提供することを目的に設定されたアフィリエイト宣伝プログラムである、
          Amazonアソシエイト・プログラムの参加者です。
        </p>
      </Col>
    </Row>
  </Layout>
)

export default AboutPage

export const pageQuery = graphql`
  query {
    site {
      siteMetadata {
        title
      }
    }
  }
`
