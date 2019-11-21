import React from 'react'
import {
  Row,
  Col,
  FormGroup,
  Button
} from 'reactstrap'
import Layout from '../components/organisms/layout'

const ContactPage = () => (
  <Layout title="お問い合わせ">
    <Row>
      <Col md={12}>
        <form name="contact" method="POST" data-netlify="true" data-netlify-honeypot="bot-field">
          <input type="hidden" name="form-name" value="contact" />
          <FormGroup>
            <label for="name" className="control-label">お名前</label>
            <input name="name" type="text" className="form-control"/>
          </FormGroup>
          <FormGroup>
            <label for="email" className="control-label">メールアドレス</label>
            <input name="email" type="email" className="form-control"/>
          </FormGroup>
          <FormGroup>
            <label for="message" className="control-label">お問い合わせ内容</label>
            <textarea name="message" className="form-control" required>
            </textarea>
          </FormGroup>
          <FormGroup>
            <Button type="submit" color="secondary">送信</Button>
          </FormGroup>
        </form>
      </Col>
    </Row>
  </Layout>
)

export default ContactPage
